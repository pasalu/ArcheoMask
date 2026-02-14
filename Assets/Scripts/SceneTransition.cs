using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles smooth scene transitions with fade to black effects
/// </summary>
public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool isTransitioning = false;
    private static SceneTransition instance;

    private void Awake()
    {
        Debug.Log("SceneTransition Awake called");

        // Singleton pattern
        if (instance != null && instance != this)
        {
            Debug.Log("Destroying duplicate SceneTransition");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Always create the fade canvas immediately
        CreateFadeCanvas();

        Debug.Log($"SceneTransition initialized. FadeImage null? {fadeImage == null}");
    }

    /// <summary>
    /// Create the fade canvas with a black overlay image
    /// </summary>
    private void CreateFadeCanvas()
    {
        Debug.Log("=== CreateFadeCanvas START ===");

        // Create canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform, false);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("Canvas created");

        // Create image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Transparent black
        fadeImage.raycastTarget = false; // Don't block clicks when transparent

        // Make it fullscreen
        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Debug.Log($"=== CreateFadeCanvas COMPLETE === FadeImage created: {fadeImage != null}, Color: {fadeImage.color}");
    }

    /// <summary>
    /// Transition to a new scene with fade effect
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        Debug.Log($"=== TransitionToScene called: {sceneName} ===");

        if (fadeImage == null)
        {
            Debug.LogError("CRITICAL: fadeImage is NULL at start of transition!");
            CreateFadeCanvas();
        }

        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneName));
        }
        else
        {
            Debug.LogWarning("Already transitioning!");
        }
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;
        Debug.Log(">>> Starting TransitionCoroutine");

        // Fade out to black
        Debug.Log(">>> Step 1: Fade out");
        yield return StartCoroutine(FadeToBlack());

        // Load scene
        Debug.Log($">>> Step 2: Loading scene {sceneName}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log(">>> Step 3: Scene loaded");

        // Small delay
        yield return new WaitForSeconds(0.1f);

        // Fade in from black
        Debug.Log(">>> Step 4: Fade in");
        yield return StartCoroutine(FadeFromBlack());

        Debug.Log(">>> TransitionCoroutine COMPLETE");
        isTransitioning = false;
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeToBlack: fadeImage is NULL!");
            yield break;
        }

        Debug.Log($"FadeToBlack starting from alpha: {fadeImage.color.a}");

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
        Debug.Log($"FadeToBlack complete. Final alpha: {fadeImage.color.a}");
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeFromBlack: fadeImage is NULL!");
            yield break;
        }

        Debug.Log($"FadeFromBlack starting from alpha: {fadeImage.color.a}");

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        Debug.Log($"FadeFromBlack complete. Final alpha: {fadeImage.color.a}");
    }
}
