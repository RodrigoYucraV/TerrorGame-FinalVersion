using System;
using UnityEngine;

public class AttackStateLogic : IEnemyState
{
    private readonly EnemyController enemy;
    private readonly EnemyStateMachine stateMachine;
    private readonly IPlayerPositionProvider playerPositionProvider;
    private readonly IEnemyMovement movement;

    private float attackCooldown = 1.5f;
    private float lastAttackTime = -999f;
    private float attackRange = 2.2f;
    private float attackDamage = 25f;
    private AudioClip attackSound;
    private AudioSource audioSource;

    public AttackStateLogic(EnemyController enemy, EnemyStateMachine stateMachine, IPlayerPositionProvider playerPositionProvider, IEnemyMovement movement, AudioSource audioSource, AudioClip attackSound)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
        this.playerPositionProvider = playerPositionProvider;
        this.movement = movement;
        this.audioSource = audioSource;
        this.attackSound = attackSound;
    }

    public void Enter()
    {
        if (movement != null) movement.Stop();
    }

    public void Tick()
    {
        if (playerPositionProvider == null) return;

        Vector3 playerPos = playerPositionProvider.PlayerBodyPosition;
        float distance = Vector3.Distance(enemy.transform.position, playerPos);

        // Si el jugador se aleja, volver a acecho/persecución
        if (distance > attackRange + 0.8f)
        {
            enemy.GoToStealthStatePublic();
            return;
        }

        // Enfocar la mirada hacia el jugador
        Vector3 dir = (playerPos - enemy.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }

        // Lógica de ataque con cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    public void Exit()
    {
        // Limpieza si es necesaria
    }

    private void PerformAttack()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Debug.Log("[Enemy] ¡Atacando al jugador!");

        if (PlayerManager.Instance != null)
        {
            IDamageable playerDamageable = PlayerManager.Instance.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(attackDamage);
            }
        }
    }
}
