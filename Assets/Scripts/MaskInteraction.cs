using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Handles clicking the mask SpriteRenderer to reveal a hidden layer with yellow filter
/// Attach this to the Mask SpriteRenderer GameObject
/// Uses the new Input System
/// </summary>
public class MaskInteraction : MonoBehaviour
{
    [Header("Yellow Filter Reference")]
    [Tooltip("Reference to the UI Image that serves as the yellow filter")]
    [SerializeField] private Image yellowFilterImage;

    [Header("Mask Sprites")]
    [Tooltip("The mask sprite (shown when inactive)")]
    [SerializeField] private Sprite maskSprite;
    [Tooltip("The exit button sprite (shown when active)")]
    [SerializeField] private Sprite exitButtonSprite;

    [Header("Settings")]
    [SerializeField] private Color yellowFilterColor = new Color(1f, 0.9f, 0.3f, 0.3f);
    [SerializeField] private LayerMask clickableLayer = -1; // All layers by default

    private SpriteRenderer spriteRenderer;
    private bool isMaskActive = false;
    private Camera mainCamera;
    private Collider2D maskCollider;

    private void Awake()
    {
        // Get the SpriteRenderer component on this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("MaskInteraction requires a SpriteRenderer component!");
            return;
        }

        // Get the Collider2D component
        maskCollider = GetComponent<Collider2D>();
        if (maskCollider == null)
        {
            Debug.LogError("MaskInteraction requires a Collider2D component!");
            return;
        }

        // Get main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        // Set initial sprite to mask
        if (maskSprite != null)
        {
            spriteRenderer.sprite = maskSprite;
        }

        // Set up yellow filter
        if (yellowFilterImage != null)
        {
            yellowFilterImage.color = yellowFilterColor;
        }

        // Start with mask visible, filter and hidden layer off
        DeactivateMask();
    }

    private void Update()
    {
        // Check for click using new Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckForClick();
        }
    }

    /// <summary>
    /// Check if the click hit the mask collider
    /// </summary>
    private void CheckForClick()
    {
        // Get mouse position in world space
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Raycast to see if we hit the mask collider
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, clickableLayer);

        if (hit.collider != null && hit.collider == maskCollider)
        {
            Debug.Log("Mask clicked!");
            ToggleMask();
        }
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
    /// Activate mask mode: show yellow filter and change to exit button
    /// </summary>
    public void ActivateMask()
    {
        isMaskActive = true;

        // Show yellow filter
        if (yellowFilterImage != null)
        {
            yellowFilterImage.gameObject.SetActive(true);
        }

        // Change mask sprite to exit button
        if (spriteRenderer != null && exitButtonSprite != null)
        {
            spriteRenderer.sprite = exitButtonSprite;
        }

        Debug.Log("Mask activated - yellow filter revealed");
    }

    /// <summary>
    /// Deactivate mask mode: hide yellow filter and show mask
    /// </summary>
    public void DeactivateMask()
    {
        isMaskActive = false;

        // Hide yellow filter
        if (yellowFilterImage != null)
        {
            yellowFilterImage.gameObject.SetActive(false);
        }

        // Change exit button back to mask sprite
        if (spriteRenderer != null && maskSprite != null)
        {
            spriteRenderer.sprite = maskSprite;
        }

        Debug.Log("Mask deactivated - back to normal view");
    }

    /// <summary>
    /// Change the yellow filter color at runtime
    /// </summary>
    public void SetFilterColor(Color color)
    {
        yellowFilterColor = color;
        if (yellowFilterImage != null)
        {
            yellowFilterImage.color = color;
        }
    }

    /// <summary>
    /// Check if mask is currently active
    /// </summary>
    public bool IsMaskActive => isMaskActive;
}
