using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = -1f; // if left at default in inspector, will be initialized to max

    // Sends (currentHealth, maxHealth) when health changes
    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        // Initialize and clamp current health on startup
        if (currentHealth <= 0f || currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Read-only accessors
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        SetHealth(currentHealth - amount);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        SetHealth(currentHealth + amount);
    }

    private void SetHealth(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, maxHealth);
        if (Mathf.Approximately(clamped, currentHealth)) return;

        currentHealth = clamped;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
