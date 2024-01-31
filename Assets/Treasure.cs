using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Treasure : MonoBehaviour
{
    private bool isNear;
    private bool isOpened;
    private TreasureManager treasureManager;
    public Renderer treasureRenderer;
    public Color highlightColor = Color.yellow; // Color to highlight the treasure when near

    void Start()
    {
        isNear = false;
        treasureManager = FindAnyObjectByType<TreasureManager>();

        // Ensure the renderer component exists and it uses a material with color
        if (treasureRenderer == null)
        {
            Debug.LogError("Renderer component not found or missing material with color.");
        }

        isOpened = false;
    }

    public void onPlayerEnterRange()
    {
        isNear = true;
        if (isNear)
        {
            // Change the color to highlightColor when player enters the range
            treasureRenderer.material.color = highlightColor;
        }
    }

    public void onPlayerExitRange()
    {
        isNear = false;
        if (!isNear)
        {
            // Reset the color when player exits the range
            treasureRenderer.material.color = Color.white; // Change this to the original color if known
        }
    }

    public void TryToOpenTreasure(InputAction.CallbackContext context)
    {
        if (isNear && context.started && !isOpened)
        {
            SessionManager.currentTreasure = gameObject;
            treasureManager.TriggerTreasureEvent();
            isOpened = true;
        }

    }     
}
