using UnityEngine;

/// <summary>
/// Represents a stencil that can be used to draw patterns on the canvas
/// </summary>
public class StencilObject : MonoBehaviour
{
    [Header("Stencil Settings")]
    [SerializeField] private Texture2D stencilTexture;
    [SerializeField] private string stencilName;
    [SerializeField] private Vector2 stencilSize = new Vector2(100f, 100f);

    [Header("Interaction")]
    [SerializeField] private bool isDraggable = true;
    [SerializeField] private bool isRotatable = true;
    [SerializeField] private bool isScalable = true;

    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float currentRotation = 0f;
    private Vector2 currentScale = Vector2.one;

    public Texture2D StencilTexture => stencilTexture;
    public string StencilName => stencilName;
    public Vector2 StencilSize => stencilSize * currentScale;
    public float Rotation => currentRotation;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Apply this stencil to the canvas at the current position
    /// </summary>
    public void ApplyToCanvas(DrawingCanvas canvas, Vector2 canvasPosition)
    {
        if (stencilTexture == null || canvas == null)
        {
            Debug.LogWarning("Cannot apply stencil: Missing texture or canvas");
            return;
        }

        Texture2D canvasTexture = canvas.DrawTexture;
        Vector2 scaledSize = StencilSize;

        // Calculate the starting position (centered on canvasPosition)
        int startX = Mathf.RoundToInt(canvasPosition.x - scaledSize.x / 2f);
        int startY = Mathf.RoundToInt(canvasPosition.y - scaledSize.y / 2f);

        // Apply each pixel from the stencil
        for (int x = 0; x < scaledSize.x; x++)
        {
            for (int y = 0; y < scaledSize.y; y++)
            {
                int canvasX = startX + x;
                int canvasY = startY + y;

                // Check bounds
                if (canvasX >= 0 && canvasX < canvasTexture.width &&
                    canvasY >= 0 && canvasY < canvasTexture.height)
                {
                    // Sample from stencil texture
                    float u = x / scaledSize.x;
                    float v = y / scaledSize.y;
                    Color stencilColor = stencilTexture.GetPixelBilinear(u, v);

                    // Only apply if the stencil pixel is not transparent
                    if (stencilColor.a > 0.1f)
                    {
                        canvasTexture.SetPixel(canvasX, canvasY, stencilColor);
                    }
                }
            }
        }

        canvasTexture.Apply();
    }

    /// <summary>
    /// Set the stencil texture
    /// </summary>
    public void SetStencilTexture(Texture2D texture)
    {
        stencilTexture = texture;
    }

    /// <summary>
    /// Rotate the stencil by the given angle (in degrees)
    /// </summary>
    public void Rotate(float angle)
    {
        if (!isRotatable) return;

        currentRotation += angle;
        currentRotation = Mathf.Repeat(currentRotation, 360f);
        transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
    }

    /// <summary>
    /// Scale the stencil
    /// </summary>
    public void Scale(Vector2 scale)
    {
        if (!isScalable) return;

        currentScale = new Vector2(
            Mathf.Clamp(scale.x, 0.1f, 5f),
            Mathf.Clamp(scale.y, 0.1f, 5f)
        );
        transform.localScale = new Vector3(currentScale.x, currentScale.y, 1f);
    }

    /// <summary>
    /// Get the current position in screen space
    /// </summary>
    public Vector2 GetScreenPosition()
    {
        return mainCamera.WorldToScreenPoint(transform.position);
    }

    /// <summary>
    /// Set the position from screen coordinates
    /// </summary>
    public void SetScreenPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
        transform.position = worldPosition;
    }

    // Mouse interaction methods
    private void OnMouseDown()
    {
        if (!isDraggable) return;

        isDragging = true;
        offset = transform.position - mainCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f)
        );
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        transform.position = worldPosition + offset;
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    /// <summary>
    /// Create a rotated version of the stencil texture
    /// </summary>
    public Texture2D GetRotatedTexture()
    {
        if (stencilTexture == null) return null;
        if (Mathf.Approximately(currentRotation, 0f)) return stencilTexture;

        // For now, return the original texture
        // Implementing proper rotation would require more complex texture manipulation
        return stencilTexture;
    }
}
