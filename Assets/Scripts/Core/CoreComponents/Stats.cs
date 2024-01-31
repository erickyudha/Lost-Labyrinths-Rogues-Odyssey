using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : CoreComponent
{
    public event Action OnHealthZero;
    
    public float maxHealth;
    public float currentHealth;
    public int goldValue = 0;

    protected override void Awake()
    {
        base.Awake();

        // currentHealth = maxHealth;

        if (CompareTag("PlayerMark"))
        {
            maxHealth = SessionManager.playerMaxHealth;
            currentHealth = SessionManager.playerMaxHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            
            OnHealthZero?.Invoke();
            
            Debug.Log("Health is zero!!");
        }
    }

    public void IncreaseHealth(float amount)
    {
        if (CompareTag("PlayerMark"))
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, SessionManager.playerMaxHealth);
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        }
    }
}
