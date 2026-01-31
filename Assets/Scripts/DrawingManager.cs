using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Manages the overall drawing system, coordinating canvas, stencils, and comparison
/// </summary>
public class DrawingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DrawingCanvas drawingCanvas;
    [SerializeField] private ImageComparer imageComparer;
    [SerializeField] private TargetImage currentTargetImage;

    [Header("Stencil Management")]
    [SerializeField] private List<StencilObject> availableStencils = new List<StencilObject>();
    [SerializeField] private Transform stencilContainer;

    [Header("Input Settings")]
    [SerializeField] private bool useMouseInput = true;
    [SerializeField] private bool useTouchInput = true;
    [SerializeField] private KeyCode compareKey = KeyCode.Space;
    [SerializeField] private KeyCode clearKey = KeyCode.C;

    [Header("Events")]
    public UnityEvent<ImageComparer.ComparisonResult> onComparisonComplete;
    public UnityEvent onMatchFound;
    public UnityEvent onMatchFailed;
    public UnityEvent onCanvasCleared;

    private StencilObject activeStencil;
    private bool isDrawingMode = false;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Get or create required components
        if (drawingCanvas == null)
        {
            drawingCanvas = GetComponent<DrawingCanvas>();
        }

        if (imageComparer == null)
        {
            imageComparer = GetComponent<ImageComparer>();
            if (imageComparer == null)
            {
                imageComparer = gameObject.AddComponent<ImageComparer>();
            }
        }

        // Initialize events if null
        if (onComparisonComplete == null) onComparisonComplete = new UnityEvent<ImageComparer.ComparisonResult>();
        if (onMatchFound == null) onMatchFound = new UnityEvent();
        if (onMatchFailed == null) onMatchFailed = new UnityEvent();
        if (onCanvasCleared == null) onCanvasCleared = new UnityEvent();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Clear canvas
        if (Input.GetKeyDown(clearKey))
        {
            ClearCanvas();
        }

        // Compare images
        if (Input.GetKeyDown(compareKey))
        {
            CompareWithTarget();
        }

        // Mouse/Touch drawing
        if (useMouseInput)
        {
            HandleMouseInput();
        }
    }

    private void HandleMouseInput()
    {
        if (isDrawingMode)
        {
            Vector2 canvasPos = GetCanvasPosition(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                drawingCanvas.StartDrawing(canvasPos);
            }
            else if (Input.GetMouseButton(0))
            {
                drawingCanvas.ContinueDrawing(canvasPos);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                drawingCanvas.StopDrawing();
            }
        }
    }

    /// <summary>
    /// Convert screen position to canvas texture coordinates
    /// </summary>
    private Vector2 GetCanvasPosition(Vector3 screenPosition)
    {
        // This is a simplified version - you'll need to adapt this based on your UI setup
        RectTransform canvasRect = drawingCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                null,
                out localPoint
            );

            // Convert to texture coordinates
            float x = (localPoint.x + canvasRect.rect.width / 2f);
            float y = (localPoint.y + canvasRect.rect.height / 2f);

            return new Vector2(x, y);
        }

        return Vector2.zero;
    }

    /// <summary>
    /// Set the active stencil
    /// </summary>
    public void SetActiveStencil(StencilObject stencil)
    {
        activeStencil = stencil;
    }

    /// <summary>
    /// Apply the active stencil to the canvas at the given position
    /// </summary>
    public void ApplyStencilAtPosition(Vector2 canvasPosition)
    {
        if (activeStencil != null && drawingCanvas != null)
        {
            activeStencil.ApplyToCanvas(drawingCanvas, canvasPosition);
        }
        else
        {
            Debug.LogWarning("Cannot apply stencil: No active stencil or canvas");
        }
    }

    /// <summary>
    /// Compare the current drawing with the target image
    /// </summary>
    public void CompareWithTarget()
    {
        if (currentTargetImage == null)
        {
            Debug.LogWarning("No target image set for comparison");
            return;
        }

        if (drawingCanvas == null)
        {
            Debug.LogError("No drawing canvas found");
            return;
        }

        Texture2D drawnTexture = drawingCanvas.GetCanvasTexture();
        Texture2D targetTexture = currentTargetImage.TargetTexture;

        ImageComparer.ComparisonResult result = imageComparer.CompareImages(drawnTexture, targetTexture);

        // Invoke events
        onComparisonComplete?.Invoke(result);

        if (result.isMatch)
        {
            Debug.Log($"Match found! Similarity: {result.similarityScore:P0}");
            onMatchFound?.Invoke();
        }
        else
        {
            Debug.Log($"No match. Similarity: {result.similarityScore:P0} (Required: {imageComparer.MatchThreshold:P0})");
            onMatchFailed?.Invoke();
        }

        // Clean up
        Destroy(drawnTexture);
    }

    /// <summary>
    /// Clear the drawing canvas
    /// </summary>
    public void ClearCanvas()
    {
        if (drawingCanvas != null)
        {
            drawingCanvas.ClearCanvas();
            onCanvasCleared?.Invoke();
            Debug.Log("Canvas cleared");
        }
    }

    /// <summary>
    /// Set the current target image
    /// </summary>
    public void SetTargetImage(TargetImage targetImage)
    {
        currentTargetImage = targetImage;

        // Update the comparer's threshold based on target's required accuracy
        if (imageComparer != null && targetImage != null)
        {
            imageComparer.SetMatchThreshold(targetImage.RequiredAccuracy);
        }
    }

    /// <summary>
    /// Toggle between drawing mode and stencil mode
    /// </summary>
    public void SetDrawingMode(bool enabled)
    {
        isDrawingMode = enabled;
    }

    /// <summary>
    /// Add a stencil to the available stencils list
    /// </summary>
    public void RegisterStencil(StencilObject stencil)
    {
        if (!availableStencils.Contains(stencil))
        {
            availableStencils.Add(stencil);
        }
    }

    /// <summary>
    /// Remove a stencil from the available stencils list
    /// </summary>
    public void UnregisterStencil(StencilObject stencil)
    {
        availableStencils.Remove(stencil);
    }

    /// <summary>
    /// Get all available stencils
    /// </summary>
    public List<StencilObject> GetAvailableStencils()
    {
        return new List<StencilObject>(availableStencils);
    }

    /// <summary>
    /// Get the current comparison result details
    /// </summary>
    public void ShowComparisonDetails()
    {
        if (currentTargetImage == null || drawingCanvas == null) return;

        Texture2D drawnTexture = drawingCanvas.GetCanvasTexture();
        Texture2D targetTexture = currentTargetImage.TargetTexture;

        Texture2D diffMap = imageComparer.GetDifferenceMap(drawnTexture, targetTexture);

        // You can display this difference map in your UI
        Debug.Log("Difference map generated");

        Destroy(drawnTexture);
        Destroy(diffMap);
    }

    /// <summary>
    /// Save the current drawing
    /// </summary>
    public void SaveDrawing(string filename)
    {
        if (drawingCanvas == null) return;

        Texture2D texture = drawingCanvas.GetCanvasTexture();
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/" + filename + ".png", bytes);

        Debug.Log($"Drawing saved to {Application.persistentDataPath}/{filename}.png");

        Destroy(texture);
    }

    /// <summary>
    /// Load a drawing
    /// </summary>
    public void LoadDrawing(string filepath)
    {
        if (!System.IO.File.Exists(filepath))
        {
            Debug.LogError($"File not found: {filepath}");
            return;
        }

        byte[] bytes = System.IO.File.ReadAllBytes(filepath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);

        // Apply to canvas
        // You'll need to implement this method in DrawingCanvas
        Debug.Log($"Drawing loaded from {filepath}");
    }
}
