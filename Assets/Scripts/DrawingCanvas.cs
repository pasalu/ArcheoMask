using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles drawing onto a RenderTexture using stencils and input
/// </summary>
public class DrawingCanvas : MonoBehaviour
{
    [Header("Canvas Settings")]
    [SerializeField] private RawImage canvasImage;
    [SerializeField] private int textureWidth = 1024;
    [SerializeField] private int textureHeight = 1024;
    [SerializeField] private Texture2D backgroundImage; // Optional background image (like a_stone_tablet.png)

    [Header("Drawing Settings")]
    [SerializeField] private Color drawColor = Color.black;
    [SerializeField] private float brushSize = 10f;

    private RenderTexture renderTexture;
    private Texture2D drawTexture;
    private Texture2D backgroundTexture; // Copy of the background for resetting
    private Camera drawCamera;
    private Vector2 lastDrawPosition;
    private bool isDrawing = false;

    public RenderTexture RenderTexture => renderTexture;
    public Texture2D DrawTexture => drawTexture;

    private void Awake()
    {
        InitializeCanvas();
    }

    private void InitializeCanvas()
    {
        // If background image is provided, use its dimensions
        if (backgroundImage != null)
        {
            textureWidth = backgroundImage.width;
            textureHeight = backgroundImage.height;
        }

        // Create the render texture
        renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.Create();

        // Create the draw texture
        drawTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Copy background image if provided
        if (backgroundImage != null)
        {
            backgroundTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            backgroundTexture.SetPixels(backgroundImage.GetPixels());
            backgroundTexture.Apply();
        }

        // Clear the texture to white (or background image)
        ClearCanvas();

        // Apply to the UI image if assigned
        if (canvasImage != null)
        {
            canvasImage.texture = drawTexture;
        }
    }

    /// <summary>
    /// Clears the canvas to a blank state (or resets to background image)
    /// </summary>
    public void ClearCanvas()
    {
        if (backgroundTexture != null)
        {
            // Reset to background image
            drawTexture.SetPixels(backgroundTexture.GetPixels());
        }
        else
        {
            // Clear to white
            Color[] clearColors = new Color[textureWidth * textureHeight];
            for (int i = 0; i < clearColors.Length; i++)
            {
                clearColors[i] = Color.white;
            }
            drawTexture.SetPixels(clearColors);
        }
        drawTexture.Apply();
    }

    /// <summary>
    /// Start drawing at the given position
    /// </summary>
    public void StartDrawing(Vector2 position)
    {
        isDrawing = true;
        lastDrawPosition = position;
        DrawAtPosition(position);
    }

    /// <summary>
    /// Continue drawing to the given position
    /// </summary>
    public void ContinueDrawing(Vector2 position)
    {
        if (!isDrawing) return;

        // Draw a line from last position to current position
        DrawLine(lastDrawPosition, position);
        lastDrawPosition = position;
    }

    /// <summary>
    /// Stop drawing
    /// </summary>
    public void StopDrawing()
    {
        isDrawing = false;
    }

    /// <summary>
    /// Draw a single point at the given position
    /// </summary>
    private void DrawAtPosition(Vector2 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);

        DrawCircle(x, y, brushSize);
        drawTexture.Apply();
    }

    /// <summary>
    /// Draw a line between two points
    /// </summary>
    private void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / (brushSize * 0.5f));

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            int x = Mathf.RoundToInt(point.x);
            int y = Mathf.RoundToInt(point.y);

            DrawCircle(x, y, brushSize);
        }

        drawTexture.Apply();
    }

    /// <summary>
    /// Draw a circle at the given position
    /// </summary>
    private void DrawCircle(int centerX, int centerY, float radius)
    {
        int radiusInt = Mathf.RoundToInt(radius);

        for (int x = -radiusInt; x <= radiusInt; x++)
        {
            for (int y = -radiusInt; y <= radiusInt; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = centerX + x;
                    int drawY = centerY + y;

                    if (drawX >= 0 && drawX < textureWidth && drawY >= 0 && drawY < textureHeight)
                    {
                        drawTexture.SetPixel(drawX, drawY, drawColor);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set the current drawing color
    /// </summary>
    public void SetDrawColor(Color color)
    {
        drawColor = color;
    }

    /// <summary>
    /// Set the brush size
    /// </summary>
    public void SetBrushSize(float size)
    {
        brushSize = Mathf.Max(1f, size);
    }

    /// <summary>
    /// Get a copy of the current canvas as Texture2D
    /// </summary>
    public Texture2D GetCanvasTexture()
    {
        Texture2D copy = new Texture2D(textureWidth, textureHeight);
        copy.SetPixels(drawTexture.GetPixels());
        copy.Apply();
        return copy;
    }

    /// <summary>
    /// Set or change the background image
    /// </summary>
    public void SetBackgroundImage(Texture2D newBackground)
    {
        backgroundImage = newBackground;

        if (newBackground != null)
        {
            // Update texture dimensions if needed
            if (backgroundTexture != null)
            {
                Destroy(backgroundTexture);
            }

            textureWidth = newBackground.width;
            textureHeight = newBackground.height;

            backgroundTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            backgroundTexture.SetPixels(newBackground.GetPixels());
            backgroundTexture.Apply();

            // Reset canvas to show new background
            ClearCanvas();
        }
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (drawTexture != null)
        {
            Destroy(drawTexture);
        }

        if (backgroundTexture != null)
        {
            Destroy(backgroundTexture);
        }
    }
}
