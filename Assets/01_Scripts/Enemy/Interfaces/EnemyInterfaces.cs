using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public interface IPlayerDetector
{
    bool IsPlayerLookingAtEnemy { get; }
    bool IsHitByFlashlight { get; }
}

public interface IPlayerDetectionEvents
{
    event Action OnPlayerDetected; // Se dispara cuando el jugador MIRA al enemigo
    event Action OnPlayerLost;     // Se dispara cuando el jugador DEJA DE MIRAR
    event Action OnEnemyAttack;
    event Action OnHitByFlashlight;
}

public interface IEnemyMovementConfig
{
    float FollowSpeed { get; }
    float ChaseSpeed { get; }
    float DesiredFollowDistance { get; }
}

public interface IFollowSettings
{
    float FollowDistance { get; }
    float LateralAngle { get; }
    float ApproachDistance { get; }
    float ChaseThreshold { get; }
    Vector2 OffsetAngleRange { get; }
    float FollowSpeed { get; }
    float ChaseSpeed { get; }
}

public interface IEnemyConfig : IEnemyMovementConfig, IFollowSettings, IPlayerDetector
{
    // Puedes agregar propiedades específicas adicionales aquí
}

public interface IEnemyState
{
    void Enter();
    void Exit();
    void Tick(); //Update manual
}

public interface IEnemyMovement
{
    void MoveTo(Vector3 targetPosition, float speed);
    void Stop();
}

public interface IPlayerBodyPositionProvider
{
    Vector3 PlayerBodyPosition { get; }
    Vector3 PlayerForward { get; }
}

public interface IPlayerCameraPositionProvider
{
    Vector3 CameraPosition { get; }
    Vector3 CameraForward { get; }
}

public interface IPlayerPositionProvider : IPlayerBodyPositionProvider, IPlayerCameraPositionProvider
{
   
}

public interface IPathCalculator
{
    Vector3 CalculateDestination(Vector3 playerPosition, Vector3 playerForward, Vector3 enemyPosition, IFollowSettings settings);
}

// IFogEffect.cs - Principio de Segregación de Interfaces
public interface IFogEffect
{
    bool IsFogActive { get; }
    void StartFogEffect();
    void StopFogEffect();
    event Action OnFogComplete;
}

// Controla la apariencia visual del enemigo. (S)
public interface IVisibilityController
{

    bool IsVisible { get; }
    void SetVisibility(bool visible);
    //efecto de quemado/transparencia).
    void FadeOut(float duration);
    /// Inicia la transición para hacer aparecer al enemigo.
    void FadeIn(float duration);
}

public interface IRespawnHandler
{
    Vector3 GetRespawnPosition();
    void Respawn(Transform enemyTransform);
    /// Se dispara cuando el enemigo ha sido movido a su nueva posición.

    event Action OnRespawnComplete;
}

public interface IFlashlightConfig
{
    // Duración total del efecto de "quemado" antes de la reaparición.
    float BurnDuration { get; }
    // Velocidad a la que el enemigo retrocede
    float RetreatSpeed { get; }
}
public interface IDamageable
{
    float CurrentHealth { get; }
    void TakeDamage(float amount);
    void Heal(float amount);
    event Action OnHealthDepleted;
    event Action<float> OnDamageTaken;
}
public interface ISafeZoneProvider
{
    // Calcula la mejor posición de huida basada en la posición actual
    Vector3 GetRetreatPosition(Vector3 enemyPosition, Vector3 playerPosition);
}

// Nueva interfaz para proveer estadísticas dinámicamente
public interface IEnemyStatsProvider
{
    IEnemyMovementConfig CurrentMovementConfig { get; }
    IFollowSettings CurrentFollowSettings { get; }
}

// Interfaz para notificar progreso del juego (Ritmo)
public interface IGameProgressObserver
{
    void OnObjectiveCollected(int collected, int total);
}

