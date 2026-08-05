using Mono.Cecil;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static List<SpawnPoint> AllPoints { get; private set; } = new List<SpawnPoint>();

    private void OnEnable()
    {
        // Apenas este objeto se activa en la escena, se apunta en la lista
        AllPoints.Add(this);
    }

    private void OnDisable()
    {
        // Si el objeto se destruye o desactiva, se borra de la lista para evitar errores
        AllPoints.Remove(this);
    }
    // --------------------------------------

    [Header("Configuración")]
    public SpawnCategory category = SpawnCategory.General;
    public bool IsOccupied { get; private set; } = false;

    public void SetOccupied(bool state)
    {
        IsOccupied = state;
    }

    private void OnDrawGizmos()
    {
        // (Tu código de gizmos anterior...)
        Gizmos.color = IsOccupied ? Color.red : Color.blue;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}