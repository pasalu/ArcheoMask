using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles clicking the mask to reveal a hidden layer with yellow filter
/// </summary>
public class MaskInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image maskImage;
    [SerializeField] private Image exitButtonImage;
    [SerializeField] private GameObject yellowFilterPanel;
    [SerializeField] private GameObject hiddenLayerContainer;

    [Header("Mask/Exit Button Images")]
    [SerializeField] private Sprite maskSprite;
    [SerializeField] private Sprite exitButtonSprite;

    [Header("Yellow Filter Settings")]
    [SerializeField] private Color yellowFilterColor = new Color(1f, 0.9f, 0.3f, 0.3f);

    [Header("Hidden Layer Sprites")]
    [Tooltip("Sprites that appear on top of the tablet when mask is active")]
    [SerializeField] private Sprite[] hiddenSprites;

    private bool isMaskActive = false;
    private Image filterImage;

    private void Awake()
    {
        // If maskImage not assigned, try to get it from this GameObject
        if (maskImage == null)
        {
            maskImage = GetComponent<Image>();
        }

        // Create yellow filter panel if not assigned
        if (yellowFilterPanel == null)
        {
            CreateYellowFilterPanel();
        }

        // Create hidden layer container if not assigned
        if (hiddenLayerContainer == null)
        {
            CreateHiddenLayerContainer();
        }

        // Start with mask visible, filter and hidden layer off
        DeactivateMask();
    }

    /// <summary>
    /// Called when the mask image is clicked
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleMask();
    }

    /// <summary>
    /// Toggle between mask mode and normal mode
    /// </summary>
    public void ToggleMask()
    {
        if (isMaskActive)
        {
            DeactivateMask();
        }
        else
        {
            ActivateMask();
        }
    }

    /// <summary>
    /// Activate mask mode: show yellow filter, hidden layer, and exit button
    /// </summary>
    public void ActivateMask()
    {
        isMaskActive = true;

        // Show yellow filter
        if (yellowFilterPanel != null)
        {
            yellowFilterPanel.SetActive(true);
        }

        // Show hidden layer
        if (hiddenLayerContainer != null)
        {
            hiddenLayerContainer.SetActive(true);
        }

        // Change mask image to exit button
        if (maskImage != null && exitButtonSprite != null)
        {
            maskImage.sprite = exitButtonSprite;
        }

        Debug.Log("Mask activated - yellow filter and hidden layer revealed");
    }

    /// <summary>
    /// Deactivate mask mode: hide yellow filter and hidden layer, show mask
    /// </summary>
    public void DeactivateMask()
    {
        isMaskActive = false;

        // Hide yellow filter
        if (yellowFilterPanel != null)
        {
            yellowFilterPanel.SetActive(false);
        }

        // Hide hidden layer
        if (hiddenLayerContainer != null)
        {
            hiddenLayerContainer.SetActive(false);
        }

        // Change exit button back to mask image
        if (maskImage != null && maskSprite != null)
        {
            maskImage.sprite = maskSprite;
        }

        Debug.Log("Mask deactivated - back to normal view");
    }

    /// <summary>
    /// Create the yellow filter panel programmatically
    /// </summary>
    private void CreateYellowFilterPanel()
    {
        // Find the main Canvas
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogWarning("No Canvas found - yellow filter cannot be created");
            return;
        }

        // Create full-screen yellow overlay
        GameObject filterObj = new GameObject("YellowFilter");
        filterObj.transform.SetParent(mainCanvas.transform, false);

        RectTransform filterRect = filterObj.AddComponent<RectTransform>();
        filterRect.anchorMin = Vector2.zero;
        filterRect.anchorMax = Vector2.one;
        filterRect.sizeDelta = Vector2.zero;
        filterRect.anchoredPosition = Vector2.zero;

        filterImage = filterObj.AddComponent<Image>();
        filterImage.color = yellowFilterColor;
        filterImage.raycastTarget = false; // Don't block clicks

        yellowFilterPanel = filterObj;

        // Make sure it's behind interactive elements but in front of background
        filterRect.SetAsFirstSibling();
    }

    /// <summary>
    /// Create the hidden layer container programmatically
    /// </summary>
    private void CreateHiddenLayerContainer()
    {
        // Find the main Canvas
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogWarning("No Canvas found - hidden layer cannot be created");
            return;
        }

        // Create container for hidden sprites
        GameObject containerObj = new GameObject("HiddenLayerContainer");
        containerObj.transform.SetParent(mainCanvas.transform, false);

        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;

        hiddenLayerContainer = containerObj;

        // Create image objects for each hidden sprite
        if (hiddenSprites != null && hiddenSprites.Length > 0)
        {
            for (int i = 0; i < hiddenSprites.Length; i++)
            {
                if (hiddenSprites[i] != null)
                {
                    CreateHiddenSpriteImage(containerObj, hiddenSprites[i], i);
                }
            }
        }
    }

    /// <summary>
    /// Create an individual hidden sprite image
    /// </summary>
    private void CreateHiddenSpriteImage(GameObject parent, Sprite sprite, int index)
    {
        GameObject spriteObj = new GameObject($"HiddenSprite_{index}");
        spriteObj.transform.SetParent(parent.transform, false);

        RectTransform spriteRect = spriteObj.AddComponent<RectTransform>();

        // Position in center by default - you can customize this per sprite
        spriteRect.anchorMin = new Vector2(0.5f, 0.5f);
        spriteRect.anchorMax = new Vector2(0.5f, 0.5f);
        spriteRect.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
        spriteRect.anchoredPosition = Vector2.zero;

        Image spriteImage = spriteObj.AddComponent<Image>();
        spriteImage.sprite = sprite;
        spriteImage.raycastTarget = false;
        spriteImage.SetNativeSize();
    }

    /// <summary>
    /// Manually set the yellow filter panel reference
    /// </summary>
    public void SetYellowFilterPanel(GameObject panel)
    {
        yellowFilterPanel = panel;
        if (panel != null)
        {
            filterImage = panel.GetComponent<Image>();
        }
    }

    /// <summary>
    /// Manually set the hidden layer container reference
    /// </summary>
    public void SetHiddenLayerContainer(GameObject container)
    {
        hiddenLayerContainer = container;
    }

    /// <summary>
    /// Add a sprite to the hidden layer at runtime
    /// </summary>
    public void AddHiddenSprite(Sprite sprite, Vector2 position, Vector2 size)
    {
        if (hiddenLayerContainer == null)
        {
            Debug.LogWarning("Hidden layer container not set");
            return;
        }

        GameObject spriteObj = new GameObject($"HiddenSprite_{sprite.name}");
        spriteObj.transform.SetParent(hiddenLayerContainer.transform, false);

        RectTransform spriteRect = spriteObj.AddComponent<RectTransform>();
        spriteRect.anchorMin = new Vector2(0.5f, 0.5f);
        spriteRect.anchorMax = new Vector2(0.5f, 0.5f);
        spriteRect.anchoredPosition = position;
        spriteRect.sizeDelta = size;

        Image spriteImage = spriteObj.AddComponent<Image>();
        spriteImage.sprite = sprite;
        spriteImage.raycastTarget = false;
    }

    /// <summary>
    /// Change the yellow filter color
    /// </summary>
    public void SetFilterColor(Color color)
    {
        yellowFilterColor = color;
        if (filterImage != null)
        {
            filterImage.color = color;
        }
    }

    public bool IsMaskActive => isMaskActive;
}
