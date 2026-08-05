using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(Image))]
public class HealthUI : MonoBehaviour
{
    [SerializeField] private RectTransform healthBarFill;
    private IDamageable _damageable;

    public void Initialize(IDamageable damageable)
    {
        _damageable = damageable;
        //_damageable.OnDeath += OnDeath;
        //UpdateHealth(_damageable.HealthPct);
    }

    private void Update()
    {
        if (_damageable != null)
        {
            //UpdateHealth(_damageable.HealthPct);
        }
    }

    private void UpdateHealth(float pct)
    {
        healthBarFill.localScale = new Vector3(1f, pct, 1f);
    }

    private void OnDeath()
    {
        // Opcional: Efectos al morir
    }
}