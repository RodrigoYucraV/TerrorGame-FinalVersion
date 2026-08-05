using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISanityAffector
{
    float SanityEffectPerSecond { get; } // Valores negativos drenan, positivos restauran
    bool IsAffecting { get; }
    Vector3 AffectOrigin { get; } // Para efectos de distancia
}
