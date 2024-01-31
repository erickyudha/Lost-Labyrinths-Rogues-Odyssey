using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathManager : MonoBehaviour
{
    public GameObject deathScreenCanvas;
    public GameObject mainMenuBtn;
    public float fadeInDuration = 1.0f;
    public float buttonFadeInDelay = 0.2f;
    public float btnFadeInDuration = 0.2f;
    public AudioSource bgm;

    void Awake()
    {
        mainMenuBtn.SetActive(false);
        deathScreenCanvas.SetActive(false);
    }

    public void TriggerDeathScreen()
    {
        bgm.Stop();
        StartCoroutine(FadeInCanvasAndButton());
    }

    IEnumerator FadeInCanvasAndButton()
    {
        // Activate the death screen canvas
        deathScreenCanvas.SetActive(true);

        // Fade in the canvas gradually
        CanvasGroup canvasGroup = deathScreenCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // Wait for a delay before fading in the button
        yield return new WaitForSeconds(buttonFadeInDelay);

        // Activate and fade in the main menu button
        mainMenuBtn.SetActive(true);
        Image buttonImage = mainMenuBtn.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.canvasRenderer.SetAlpha(0f);
            buttonImage.CrossFadeAlpha(1f, btnFadeInDuration, true);
        }
    }
}
