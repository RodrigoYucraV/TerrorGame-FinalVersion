using UnityEngine;
using System;

// Definimos las fases claramente
public enum EnemyPhase
{
    Phase1_Stealth, // Solo camina
    Phase2_Jumper,  // Puede saltar
    Phase3_Flyer    // Vuela / Modo Rabia
}

public class EnemyEvolutionManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private EnemyController controller;
    public EnemyPhase CurrentPhase { get; private set; } = EnemyPhase.Phase1_Stealth;
    public event Action<EnemyPhase> OnPhaseChanged;

    // ? MÉTODO PÚBLICO: Este es el único punto de entrada.
    // El "Nivel" llamará a este método cuando el jugador cruce una puerta o active un evento.
    public void SetPhase(EnemyPhase newPhase)
    {
        if (CurrentPhase == newPhase) return; // Evitar redundancia

        CurrentPhase = newPhase;
        Debug.Log($"<color=magenta>DIRECTOR DEL NIVEL: Evolucionando enemigo a {newPhase}</color>");

        // 1. Notificar al Controller para que cambie su lógica interna
        controller.HandlePhaseChange(newPhase);

        // 2. Disparar eventos visuales
        OnPhaseChanged?.Invoke(newPhase);
    }
}