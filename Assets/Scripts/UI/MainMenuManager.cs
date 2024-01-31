using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsCanvas;
    public static bool isSettingsOpen;

    // Start is called before the first frame update
    void Awake()
    {
        isSettingsOpen = false;
        settingsCanvas.SetActive(false);
    }

    private void OpenSettings()
    {
        settingsCanvas.SetActive(true);
    }

    private void CloseSettings()
    {
        settingsCanvas.SetActive(false);
    }
    public void toggleSettingsMenu()
    {
        if (isSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }

            isSettingsOpen = !isSettingsOpen;
    }

    public void toggleSettingsMenu(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }

            isSettingsOpen = !isSettingsOpen;
        }
    }

    public void StartNewGame()
    {
        SessionManager.StartNewSession();
        SessionManager.LoadNextLevelWithLoadingScreen();
    }

    public void Exit()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }
}
