using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Shield Settings")]
    public float maxShield = 0f;
    public float currentShield = 0f;

    [Header("Armor Settings")]
    public float flatArmor = 0f;
    public float percentArmor = 0f;

    [Header("Resistances")]
    public List<ResistanceEntry> resistances = new List<ResistanceEntry>();

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onDamageTaken;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
    }

    public void TakeDamage(DamageInfo info)
    {
        if (isDead) return;

        float finalDamage = CalculateFinalDamage(info);

        // Aplicar primero al escudo
        if (currentShield > 0f)
        {
            float shieldDamage = Mathf.Min(currentShield, finalDamage);
            currentShield -= shieldDamage;
            finalDamage -= shieldDamage;

            if (finalDamage <= 0f)
            {
                onDamageTaken?.Invoke(shieldDamage);
                return;
            }
        }

        // Aplicar a vida
        currentHealth -= finalDamage;
        onDamageTaken?.Invoke(finalDamage);

        if (currentHealth <= 0f)
        {
            Die(info.source);
        }
    }

    private float CalculateFinalDamage(DamageInfo info)
    {
        float damage = info.amount;

        // 1. Aplicar resistencia
        float resistanceMultiplier = 1f;
        foreach (var res in resistances)
        {
            if (res.damageType == info.damageType)
            {
                resistanceMultiplier = res.resistanceMultiplier;
                break;
            }
        }

        damage *= resistanceMultiplier;

        // 2. Aplicar armadura
        damage -= flatArmor;
        damage *= (1f - percentArmor);

        return Mathf.Max(damage, 0f);
    }

    private void Die(GameObject killer)
    {
        isDead = true;
        currentHealth = 0f;
        onDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void RechargeShield(float amount)
    {
        if (isDead) return;

        currentShield = Mathf.Min(currentShield + amount, maxShield);
    }

    public void SetMaxHealth(float newMaxHealth, bool resetToFull = true)
    {
        maxHealth = newMaxHealth;
        if (resetToFull) currentHealth = maxHealth;
    }
}

[System.Serializable]
public class ResistanceEntry
{
    public string damageType;
    [Range(0f, 2f)]
    public float resistanceMultiplier = 1.0f;
}
