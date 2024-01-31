using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public int damageAmount = 10; // Amount of damage to apply
    public float knockbackAmount = 10f; // Amount of damage to apply
    public float damageInterval = 1.0f; // Time interval between each damage application

    private float lastDamageTime; // Time of the last damage application

    void Start()
    {
        lastDamageTime = -damageInterval; // Set initial time for damage interval calculation
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Check if the collision is with the player
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // Check if the time since last damage is greater than the damage interval
            if (Time.time - lastDamageTime > damageInterval)
            {
                // Apply damage to the player
                GameObject collidedObject = other.gameObject;
                Core objectCore = collidedObject.GetComponentInChildren<Core>();

                objectCore.GetCoreComponent<Combat>().Damage(damageAmount);
                objectCore.GetCoreComponent<Combat>().Knockback(Vector2.up, knockbackAmount, objectCore.GetCoreComponent<Movement>().FacingDirection);

                lastDamageTime = Time.time; // Update the last damage time
            }
        }
    }
}
