using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class StealthFollowStateLogic: IEnemyState
{
    private readonly EnemyController enemy;
    private readonly IPlayerDetectionEvents detectionEvents;
    private readonly IEnemyMovement movement;
    private readonly IFollowSettings followSettings;
    private readonly IEnemyMovementConfig movementConfig;
    private readonly IPlayerPositionProvider playerPositionProvider;

    private bool isMovementStopped;
    private bool ignoreSight; // Nuevo flag para el nivel final
    private float debugDistanceTimer;

    // Constructor actualizado
    public StealthFollowStateLogic(
        EnemyController enemy,
        IPlayerDetector detector,
        IPlayerPositionProvider positionProvider,
        IFollowSettings settings,
        IEnemyMovementConfig moveConfig,
        bool ignoreSight) // <--- Nuevo parámetro
    {
        this.enemy = enemy;
        this.movement = enemy.GetComponent<IEnemyMovement>();
        this.followSettings = settings;
        this.movementConfig = moveConfig;
        this.playerPositionProvider = positionProvider;
        this.ignoreSight = ignoreSight;

        // Casteamos una sola vez, pero NO nos suscribimos aquí
        this.detectionEvents = detector as IPlayerDetectionEvents;
    }

    public void Enter()
    {
        // ✅ CORRECCIÓN CLAVE: Suscribirse SOLO al entrar al estado
        if (detectionEvents != null)
        {
            detectionEvents.OnPlayerDetected += HandlePlayerDetected;
            detectionEvents.OnPlayerLost += HandlePlayerLost;
        }
        isMovementStopped = false;
    }

    public void Exit()
    {
        // ✅ CORRECCIÓN CLAVE: Desuscribirse al salir (Ir a Huida/Retreat)
        // Esto garantiza que la lógica de "pararse" no interfiera con la huida
        if (detectionEvents != null)
        {
            detectionEvents.OnPlayerDetected -= HandlePlayerDetected;
            detectionEvents.OnPlayerLost -= HandlePlayerLost;
        }
        movement.Stop();
    }

    public void Tick()
    {
        if (playerPositionProvider == null)
            return;

        if (!ignoreSight &&
            detectionEvents is IPlayerDetector detector &&
            detector.IsHitByFlashlight)
        {
            enemy.HandleFlashlightReaction();
            return;
        }
        if (!ignoreSight &&
            detectionEvents is IPlayerDetector playerDetector &&
            playerDetector.IsPlayerLookingAtEnemy)
        {
            isMovementStopped = true;
            movement.Stop();
            return;
        }
        isMovementStopped = false;

        float dist = Vector3.Distance(
            enemy.transform.position,
            playerPositionProvider.PlayerBodyPosition
        );


        if (dist <= 1.15f)
        {
            enemy.GoToAttackState();
            return;
        }

        CalculateAndMove();
    }

    private void HandlePlayerDetected()
    {
        // Si estamos en la fase final (ignoreSight), ignoramos que nos miren
        if (ignoreSight) return;

        isMovementStopped = true;
        movement.Stop();
    }

    private void HandlePlayerLost()
    {
        isMovementStopped = false;
    }

    private void CalculateAndMove()
    {
        Vector3 playerBodyPos = playerPositionProvider.PlayerBodyPosition;

        // --- NUEVA REGLA DE ALTURA (Verticality Check) ---
        float heightDifference = Mathf.Abs(playerBodyPos.y - enemy.transform.position.y);

        // Si el jugador está en otro piso (> 2.5m de diferencia), 
        // OLVIDA el sigilo y ve directo a él. Esto obliga al NavMesh a buscar las escaleras/saltos.
        if (heightDifference > 2.5f)
        {
            float chaseSpeed = movementConfig.ChaseSpeed;

            // Si estamos en Fase 2 (Enraged), podemos ir incluso un poco más rápido al subir
            if (ignoreSight) chaseSpeed *= 1.1f;

            movement.MoveTo(playerBodyPos, chaseSpeed);
            return; // ¡Salimos de la función aquí! No calculamos nada más.
        }
        // -------------------------------------------------

        // SI ESTAMOS EN EL MISMO PISO, APLICAMOS LA LÓGICA DE FLANQUEO ORIGINAL:

        Vector3 playerForward = playerPositionProvider.PlayerForward;
        Vector3 behindPlayer = playerBodyPos - playerForward * followSettings.FollowDistance;
        Vector3 directionToBehind = (behindPlayer - enemy.transform.position).normalized;
        Vector3 curvedDirection = Quaternion.Euler(0, followSettings.LateralAngle, 0) * directionToBehind;
        Vector3 idealDestination = behindPlayer + curvedDirection * followSettings.ApproachDistance;

        Vector3 finalDestination;

        // Validación de terreno (tu código anterior)
        if (NavMesh.SamplePosition(idealDestination, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            finalDestination = hit.position;
        }
        else
        {
            finalDestination = playerBodyPos;
        }

        float currentSpeed = Vector3.Distance(playerBodyPos, enemy.transform.position) > followSettings.ChaseThreshold
            ? movementConfig.ChaseSpeed
            : movementConfig.FollowSpeed;

        movement.MoveTo(finalDestination, currentSpeed);
    }
}


