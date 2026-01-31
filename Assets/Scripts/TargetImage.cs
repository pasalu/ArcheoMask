using UnityEngine;

/// <summary>
/// Represents a target image that the player needs to match
/// </summary>
public class TargetImage : MonoBehaviour
{
    [Header("Target Image Settings")]
    [SerializeField] private Texture2D targetTexture;
    [SerializeField] private string imageName;
    [SerializeField] private string imageDescription;

    [Header("Display Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool showInReferenceScene = true;
    [SerializeField] private float displayDuration = 5f; // How long to show in reference scene

    [Header("Difficulty")]
    [SerializeField] private float requiredAccuracy = 0.85f; // 0-1 scale
    [SerializeField] private int complexityLevel = 1; // 1-5 scale

    private bool hasBeenViewed = false;
    private float viewedTime = 0f;

    public Texture2D TargetTexture => targetTexture;
    public string ImageName => imageName;
    public string ImageDescription => imageDescription;
    public float RequiredAccuracy => requiredAccuracy;
    public int ComplexityLevel => complexityLevel;
    public bool HasBeenViewed => hasBeenViewed;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateSprite();
    }

    /// <summary>
    /// Set the target texture
    /// </summary>
    public void SetTargetTexture(Texture2D texture)
    {
        targetTexture = texture;
        UpdateSprite();
    }

    /// <summary>
    /// Update the sprite renderer with the current texture
    /// </summary>
    private void UpdateSprite()
    {
        if (targetTexture != null && spriteRenderer != null)
        {
            Sprite sprite = Sprite.Create(
                targetTexture,
                new Rect(0, 0, targetTexture.width, targetTexture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            spriteRenderer.sprite = sprite;
        }
    }

    /// <summary>
    /// Show the image to the player
    /// </summary>
    public void ShowImage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        hasBeenViewed = true;
        viewedTime = Time.time;
    }

    /// <summary>
    /// Hide the image from the player
    /// </summary>
    public void HideImage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Check if the image should still be displayed
    /// </summary>
    public bool ShouldStillBeVisible()
    {
        if (!hasBeenViewed) return false;
        return Time.time - viewedTime < displayDuration;
    }

    /// <summary>
    /// Get a copy of the target texture
    /// </summary>
    public Texture2D GetTextureCopy()
    {
        if (targetTexture == null) return null;

        Texture2D copy = new Texture2D(targetTexture.width, targetTexture.height, targetTexture.format, false);
        copy.SetPixels(targetTexture.GetPixels());
        copy.Apply();
        return copy;
    }

    /// <summary>
    /// Set the required accuracy for this target
    /// </summary>
    public void SetRequiredAccuracy(float accuracy)
    {
        requiredAccuracy = Mathf.Clamp01(accuracy);
    }

    /// <summary>
    /// Get hints about the image (for a hint system)
    /// </summary>
    public string GetHint()
    {
        if (!string.IsNullOrEmpty(imageDescription))
        {
            return imageDescription;
        }

        return $"Try to recreate the {imageName} pattern";
    }

    /// <summary>
    /// Load target image from Resources
    /// </summary>
    public void LoadFromResources(string resourcePath)
    {
        Texture2D loadedTexture = Resources.Load<Texture2D>(resourcePath);
        if (loadedTexture != null)
        {
            targetTexture = loadedTexture;
            UpdateSprite();
        }
        else
        {
            Debug.LogError($"Failed to load target image from path: {resourcePath}");
        }
    }

    /// <summary>
    /// Get metadata about this target image
    /// </summary>
    public TargetImageMetadata GetMetadata()
    {
        return new TargetImageMetadata
        {
            name = imageName,
            description = imageDescription,
            width = targetTexture != null ? targetTexture.width : 0,
            height = targetTexture != null ? targetTexture.height : 0,
            requiredAccuracy = requiredAccuracy,
            complexityLevel = complexityLevel
        };
    }

    [System.Serializable]
    public struct TargetImageMetadata
    {
        public string name;
        public string description;
        public int width;
        public int height;
        public float requiredAccuracy;
        public int complexityLevel;
    }
}
