using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Ivalue, IHealth
{
    [System.Serializable]
    public class Settings
    {
        public float MaxHealth;
    }

    public float currentHealth { get; private set; }
    public Settings settings;

    public override event Action<float> OnValueChange;

    private void Awake()
    {
        currentHealth = settings.MaxHealth;
    }
    void IHealth.Damage(float value)
    {
        ChangeHealth(-Mathf.Abs(value));
    }

    void IHealth.Regen(float value)
    {
        ChangeHealth(Mathf.Abs(value));
    }

    void ChangeHealth(float _value)
    {
        currentHealth += _value;
        OnValueChange?.Invoke(_value);
        if (currentHealth > settings.MaxHealth) currentHealth = settings.MaxHealth;
        else if (currentHealth < 0) currentHealth = 0;

    }

    public override float GetValue()
    {
        return currentHealth / settings.MaxHealth;
    }

    private void OnDestroy()
    {
        OnValueChange = null;
    }
}
