using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    private static SessionManager instance;
    private static string[] sceneListOrder = {"Main Menu", "Cave Level"};
    public static int difficulty = 50;
    public static float playerMaxHealth = 100;
    public static float playerDamageMultiplier = 1;
    public static int goldCarried = 0;
    public static int amountOfJumps = 1;
    public static float speedMultiplier = 1;
    public static float jumpHeightMultiplier = 1; 
    
    public static int currentSceneIndex = 0;

    public static Player player;

    public static GameObject currentTreasure;
    public static GameObject currentShop;
    public static int loadingProgress = 0;


    void Awake()
    {
        // Check if an instance already exists
        if (instance == null)
        {
            // If not, set the instance to this object and mark it as persistent
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }

    public static void StartNewSession()
    {
        difficulty = 50;
        playerMaxHealth = 100f;
        playerDamageMultiplier = 1f;
        goldCarried = 0;
        currentSceneIndex = 0;
        amountOfJumps = 1;
        speedMultiplier = 1;
        jumpHeightMultiplier = 1; 
    }

    public static void LoadNextLevel()
    {
        currentSceneIndex++;
        if (currentSceneIndex >= sceneListOrder.Length)
        {
            SceneManager.LoadScene(sceneListOrder[0]);
        }
        else
        {
            SceneManager.LoadScene(sceneListOrder[currentSceneIndex]);
        }
    }

    public static void LoadNextLevelWithLoadingScreen()
    {
        
        currentSceneIndex++;
        if (currentSceneIndex >= sceneListOrder.Length)
        {
            LoadSceneWithLoadingScreen(sceneListOrder[0]);
        }
        else
        {
            LoadSceneWithLoadingScreen(sceneListOrder[currentSceneIndex]);
        }
    }

    public static void LoadMainMenuWithLoadingScreen()
    {
        LoadSceneWithLoadingScreen("Main Menu");
    }

    private static void LoadSceneWithLoadingScreen(string sceneName)
    {
        SceneManager.LoadScene("Loading"); // Load the loading screen
        // Start loading the next scene asynchronously in the background

        // Allow the scene to activate immediately to start loading

        // Here you can show any loading progress or animation in your loading screen scene
        // You can use asyncLoad.progress to get the loading progress

        // Call a function to proceed to the next scene when the progress is complete
        instance.StartCoroutine(LoadNextScene(sceneName));
    }

    private static IEnumerator LoadNextScene(string sceneName)
    {
        loadingProgress = 0;
        yield return new WaitForSeconds(0.2f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        // Wait until the next scene is done loading but not yet activated
        while (!asyncLoad.isDone)
        {
            // You can display a loading progress bar or animation here based on asyncLoad.progress

            // Example: Show loading progress
            loadingProgress = (int)(asyncLoad.progress * 100);
            Debug.Log("Loading progress: " + loadingProgress + "%");

            if (asyncLoad.progress >= 0.9f)
            {
                // When the scene is almost loaded (progress at 90%),
                // allow the scene activation to switch to the next scene
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
