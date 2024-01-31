using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NextLevel : MonoBehaviour
{
    private bool isNear;
    public new Renderer renderer;
    public Color highlightColor = Color.yellow; // Color to highlight the treasure when near

    void Start()
    {
        isNear = false;
        // Ensure the renderer component exists and it uses a material with color
        if (renderer == null)
        {
            Debug.LogError("Renderer component not found or missing material with color.");
        }
    }

    public void onPlayerEnterRange()
    {
        isNear = true;
        if (isNear)
        {
            // Change the color to highlightColor when player enters the range
            renderer.material.color = highlightColor;
        }
    }

    public void onPlayerExitRange()
    {
        isNear = false;
        if (!isNear)
        {
            // Reset the color when player exits the range
            renderer.material.color = Color.white; // Change this to the original color if known
        }
    }

    public void TryGoToNextLevel(InputAction.CallbackContext context)
    {
        if (isNear && context.started)
        {
            SessionManager.LoadNextLevelWithLoadingScreen();
        }

    }     
}
