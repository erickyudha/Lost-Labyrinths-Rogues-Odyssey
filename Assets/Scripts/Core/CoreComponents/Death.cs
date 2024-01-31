using System;
using UnityEngine;

public class Death : CoreComponent
{
    [SerializeField] private GameObject[] deathParticles;

    private ParticleManager ParticleManager =>
        particleManager ? particleManager : core.GetCoreComponent(ref particleManager);
    
    private ParticleManager particleManager;

    private Stats Stats => stats ? stats : core.GetCoreComponent(ref stats);
    private Stats stats;
    public DeathManager deathManager;

    
    public void Die()
    {
        foreach (var particle in deathParticles)
        {
            ParticleManager.StartParticles(particle);
        }
        
        if (deathManager == null)
        {
            SessionManager.goldCarried += core.transform.GetComponentInChildren<Stats>().goldValue;
        }
        else
        {
            deathManager.TriggerDeathScreen();
        }

        core.transform.parent.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Stats.OnHealthZero += Die;
    }

    private void OnDisable()
    {
        Stats.OnHealthZero -= Die;
    }
}