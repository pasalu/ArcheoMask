using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Adds hover and click animations to menu buttons
/// Attach this to button GameObjects for enhanced interactions
/// </summary>
[RequireComponent(typeof(Button))]
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Color Settings")]
    [SerializeField] private bool useColorTint = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.3f); // Yellow tint
    [SerializeField] private Color pressColor = new Color(0.8f, 0.7f, 0.2f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float soundVolume = 0.5f;

    private Button button;
    private Image buttonImage;
    private Text buttonText;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Color targetColor;
    private AudioSource audioSource;
    private bool isHovering = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<Text>();
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Set initial color
        targetColor = normalColor;
        if (buttonImage != null && useColorTint)
        {
            buttonImage.color = normalColor;
        }

        // Create audio source if sounds are assigned
        if (hoverSound != null || clickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }
    }

    private void Update()
    {
        // Smooth scale animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);

        // Smooth color animation
        if (buttonImage != null && useColorTint)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        isHovering = true;
        targetScale = originalScale * hoverScale;
        targetColor = hoverColor;

        // Play hover sound
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale;
        targetColor = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        targetScale = originalScale * pressScale;
        targetColor = pressColor;

        // Play click sound
        PlaySound(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        // Return to hover state if still hovering, otherwise normal
        if (isHovering)
        {
            targetScale = originalScale * hoverScale;
            targetColor = hoverColor;
        }
        else
        {
            targetScale = originalScale;
            targetColor = normalColor;
        }
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Update button state when enabled/disabled
    /// </summary>
    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// Called when button is clicked
    /// </summary>
    private void OnButtonClicked()
    {
        // Optional: Add additional click effects here
        StartCoroutine(ClickPulseEffect());
    }

    /// <summary>
    /// Quick pulse effect on click
    /// </summary>
    private IEnumerator ClickPulseEffect()
    {
        Vector3 originalTargetScale = targetScale;

        // Quick pulse up
        targetScale = originalScale * (hoverScale * 1.05f);
        yield return new WaitForSeconds(0.05f);

        // Return to normal
        targetScale = originalTargetScale;
    }

    /// <summary>
    /// Set button to disabled appearance
    /// </summary>
    public void SetDisabled(bool disabled)
    {
        if (button != null)
        {
            button.interactable = !disabled;
        }

        if (disabled)
        {
            targetColor = disabledColor;
            targetScale = originalScale;
        }
        else
        {
            targetColor = normalColor;
        }
    }
}
