using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles smooth scene transitions with fade in/out effects
/// </summary>
public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("UI References")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private Canvas fadeCanvas;

    private bool isTransitioning = false;

    private void Awake()
    {
        // Create fade canvas if not assigned
        if (fadeCanvas == null)
        {
            CreateFadeCanvas();
        }

        // Ensure this persists between scenes (optional)
        // Uncomment if you want the transition to persist
        // DontDestroyOnLoad(gameObject);

        // Start with transparent fade image
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    /// <summary>
    /// Create the fade canvas and image
    /// </summary>
    private void CreateFadeCanvas()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform, false);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // Very high to be on top of everything

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create fade image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = fadeColor;
        fadeImage.raycastTarget = true; // Block input during transition

        Debug.Log("Fade canvas created");
    }

    /// <summary>
    /// Transition to a new scene with fade effect
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneName));
        }
    }

    /// <summary>
    /// Transition to a scene by build index
    /// </summary>
    public void TransitionToScene(int sceneIndex)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneIndex));
        }
    }

    /// <summary>
    /// Coroutine for scene transition
    /// </summary>
    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(FadeOut());

        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Fade in
        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    /// <summary>
    /// Coroutine for scene transition (by index)
    /// </summary>
    private IEnumerator TransitionCoroutine(int sceneIndex)
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(FadeOut());

        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Fade in
        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    /// <summary>
    /// Fade out to opaque
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = fadeColor;
        targetColor.a = 1f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeOutDuration);

            Color currentColor = Color.Lerp(startColor, targetColor, t);
            fadeImage.color = currentColor;

            yield return null;
        }

        // Ensure fully opaque
        fadeImage.color = targetColor;
    }

    /// <summary>
    /// Fade in to transparent
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = fadeColor;
        targetColor.a = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeInDuration);

            Color currentColor = Color.Lerp(startColor, targetColor, t);
            fadeImage.color = currentColor;

            yield return null;
        }

        // Ensure fully transparent
        fadeImage.color = targetColor;
    }

    /// <summary>
    /// Instant fade to black (useful for cuts)
    /// </summary>
    public void FadeToBlackInstant()
    {
        if (fadeImage != null)
        {
            Color color = fadeColor;
            color.a = 1f;
            fadeImage.color = color;
        }
    }

    /// <summary>
    /// Instant fade to transparent
    /// </summary>
    public void FadeToClearInstant()
    {
        if (fadeImage != null)
        {
            Color color = fadeColor;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    /// <summary>
    /// Check if currently transitioning
    /// </summary>
    public bool IsTransitioning => isTransitioning;
}
