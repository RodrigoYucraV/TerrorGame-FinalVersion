using System;
using System.Collections;
using UnityEngine;

public interface ISanityProvider
{
    float CurrentSanityPct { get; }
    event Action<float> OnSanityChanged;
    public event Action OnSanityDepleted;
}
