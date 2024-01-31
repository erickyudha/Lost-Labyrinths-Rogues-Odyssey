using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCallbackManager : MonoBehaviour
{   
    public GameObject player;
    private Stats playerStats;
    // Start is called before the first frame update
    void Start()
    {
        playerStats = player.transform.GetComponentInChildren<Stats>();
    }

    public void HealPlayer(float healAmount)
    {
        playerStats.IncreaseHealth(healAmount);
    }

    public void IncreaseDamageMultiplier(float multiplierAmount)
    {
        SessionManager.playerDamageMultiplier += multiplierAmount;
    }

    public void IncreaseSpeedMultiplier(float multiplierAmount)
    {
        SessionManager.speedMultiplier += multiplierAmount;
        PlayerData pData = player.GetComponent<Player>().playerData;
        pData.movementVelocity = pData.baseMovementVelocity * SessionManager.speedMultiplier;
    }
    
    public void IncreaseJumpMultiplier(float multiplierAmount)
    {
        SessionManager.jumpHeightMultiplier += multiplierAmount;
        PlayerData pData = player.GetComponent<Player>().playerData;
        pData.jumpVelocity = pData.baseJumpVelocity * SessionManager.jumpHeightMultiplier;
    }

    public void UseHealPotion()
    {
        HealPlayer(50);
    }

    public void UseHighHealPotion()
    {
        HealPlayer(9999);
    }

    public void UseHealthElixir()
    {
        float increaseAmount = 25;
        SessionManager.playerMaxHealth += increaseAmount;
        HealPlayer(increaseAmount);
        
        playerStats.maxHealth = SessionManager.playerMaxHealth;
    }

    public void UseLesserSword()
    {
        IncreaseDamageMultiplier(0.2f);
    }

    public void UseSword()
    {
        IncreaseDamageMultiplier(0.4f);
    }

    public void UseGreaterSword()
    {
        IncreaseDamageMultiplier(0.6f);
    }

    public void UseLesserSpeedPotion()
    {
        IncreaseSpeedMultiplier(0.1f);
    }

    public void UseMajorSpeedPotion()
    {
        IncreaseSpeedMultiplier(0.3f);
    }

    public void UseLesserJumpPotion()
    {
        IncreaseJumpMultiplier(0.2f);
    }

    public void UseMajorJumpPotion()
    {
        IncreaseJumpMultiplier(0.4f);
    }


    public void UseJumpElixir()
    {
        SessionManager.amountOfJumps += 1;
        player.GetComponent<Player>().playerData.amountOfJumps = SessionManager.amountOfJumps;
    }
}
