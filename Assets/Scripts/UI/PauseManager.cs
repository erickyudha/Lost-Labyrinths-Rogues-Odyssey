using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject gameplayCanvas;
    public GameObject pauseCanvas;
    public static bool isPaused;

    // Start is called before the first frame update
    void Awake()
    {
        Resume();
    }

    public void Pause()
    {
        gameplayCanvas.SetActive(false);
        pauseCanvas.SetActive(true);
        Time.timeScale = 0;
    }
    
    public void PauseNoUI()
    {
        gameplayCanvas.SetActive(false);
        pauseCanvas.SetActive(false);

        Time.timeScale = 0;
    }


    public void Resume()
    {
        pauseCanvas.SetActive(false);
        gameplayCanvas.SetActive(true);
        Time.timeScale = 1;
    }

    public void TogglePause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }
    }

    public void MainMenu()
    {
        Resume();
        SessionManager.LoadMainMenuWithLoadingScreen();
    }

    public void Exit()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }
}
