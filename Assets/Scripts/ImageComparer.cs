using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Compares two images and calculates similarity metrics
/// </summary>
public class ImageComparer : MonoBehaviour
{
    [Header("Comparison Settings")]
    [SerializeField] private float matchThreshold = 0.85f; // 85% similarity required
    [SerializeField] private bool useColorComparison = true;
    [SerializeField] private bool useStructuralComparison = true;
    [SerializeField] private int downsampleFactor = 2; // Reduce resolution for faster comparison

    public float MatchThreshold => matchThreshold;

    /// <summary>
    /// Result of an image comparison
    /// </summary>
    public struct ComparisonResult
    {
        public float similarityScore; // 0.0 to 1.0
        public bool isMatch;
        public float colorSimilarity;
        public float structuralSimilarity;
        public string details;
    }

    /// <summary>
    /// Compare two textures and return similarity score
    /// </summary>
    public ComparisonResult CompareImages(Texture2D drawnImage, Texture2D targetImage)
    {
        if (drawnImage == null || targetImage == null)
        {
            Debug.LogError("Cannot compare null images");
            return new ComparisonResult { similarityScore = 0f, isMatch = false, details = "Null image provided" };
        }

        ComparisonResult result = new ComparisonResult();

        // Resize images to same size if needed
        Texture2D resizedDrawn = ResizeTexture(drawnImage, targetImage.width, targetImage.height);
        Texture2D resizedTarget = targetImage;

        // Downsample for performance if needed
        if (downsampleFactor > 1)
        {
            int newWidth = resizedDrawn.width / downsampleFactor;
            int newHeight = resizedDrawn.height / downsampleFactor;
            resizedDrawn = ResizeTexture(resizedDrawn, newWidth, newHeight);
            resizedTarget = ResizeTexture(targetImage, newWidth, newHeight);
        }

        float colorScore = 0f;
        float structuralScore = 0f;

        if (useColorComparison)
        {
            colorScore = CompareColors(resizedDrawn, resizedTarget);
            result.colorSimilarity = colorScore;
        }

        if (useStructuralComparison)
        {
            structuralScore = CompareStructure(resizedDrawn, resizedTarget);
            result.structuralSimilarity = structuralScore;
        }

        // Calculate overall similarity (weighted average)
        if (useColorComparison && useStructuralComparison)
        {
            result.similarityScore = (colorScore * 0.5f + structuralScore * 0.5f);
        }
        else if (useColorComparison)
        {
            result.similarityScore = colorScore;
        }
        else if (useStructuralComparison)
        {
            result.similarityScore = structuralScore;
        }

        result.isMatch = result.similarityScore >= matchThreshold;
        result.details = $"Color: {colorScore:F2}, Structure: {structuralScore:F2}, Overall: {result.similarityScore:F2}";

        // Clean up temporary textures
        if (resizedDrawn != drawnImage && downsampleFactor > 1)
        {
            Destroy(resizedDrawn);
        }
        if (resizedTarget != targetImage && downsampleFactor > 1)
        {
            Destroy(resizedTarget);
        }

        return result;
    }

    /// <summary>
    /// Compare color similarity between two textures
    /// </summary>
    private float CompareColors(Texture2D image1, Texture2D image2)
    {
        Color[] pixels1 = image1.GetPixels();
        Color[] pixels2 = image2.GetPixels();

        if (pixels1.Length != pixels2.Length)
        {
            Debug.LogWarning("Image sizes don't match for color comparison");
            return 0f;
        }

        float totalDifference = 0f;
        int pixelCount = pixels1.Length;

        for (int i = 0; i < pixelCount; i++)
        {
            // Calculate color difference (Euclidean distance in RGB space)
            float rDiff = pixels1[i].r - pixels2[i].r;
            float gDiff = pixels1[i].g - pixels2[i].g;
            float bDiff = pixels1[i].b - pixels2[i].b;

            float pixelDifference = Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
            totalDifference += pixelDifference;
        }

        // Normalize to 0-1 range (max difference per pixel is sqrt(3))
        float averageDifference = totalDifference / pixelCount;
        float maxDifference = Mathf.Sqrt(3f);
        float similarity = 1f - (averageDifference / maxDifference);

        return Mathf.Clamp01(similarity);
    }

    /// <summary>
    /// Compare structural similarity (edge detection based)
    /// </summary>
    private float CompareStructure(Texture2D image1, Texture2D image2)
    {
        // Simple edge-based comparison
        Texture2D edges1 = DetectEdges(image1);
        Texture2D edges2 = DetectEdges(image2);

        // Compare the edge maps
        float edgeSimilarity = CompareColors(edges1, edges2);

        Destroy(edges1);
        Destroy(edges2);

        return edgeSimilarity;
    }

    /// <summary>
    /// Simple edge detection using Sobel-like operator
    /// </summary>
    private Texture2D DetectEdges(Texture2D source)
    {
        int width = source.width;
        int height = source.height;
        Texture2D edges = new Texture2D(width, height);

        Color[] pixels = source.GetPixels();

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                // Get grayscale values of surrounding pixels
                float tl = GetGrayscale(pixels[(y + 1) * width + (x - 1)]);
                float t = GetGrayscale(pixels[(y + 1) * width + x]);
                float tr = GetGrayscale(pixels[(y + 1) * width + (x + 1)]);
                float l = GetGrayscale(pixels[y * width + (x - 1)]);
                float r = GetGrayscale(pixels[y * width + (x + 1)]);
                float bl = GetGrayscale(pixels[(y - 1) * width + (x - 1)]);
                float b = GetGrayscale(pixels[(y - 1) * width + x]);
                float br = GetGrayscale(pixels[(y - 1) * width + (x + 1)]);

                // Sobel operator
                float gx = -tl - 2 * l - bl + tr + 2 * r + br;
                float gy = -bl - 2 * b - br + tl + 2 * t + tr;

                float magnitude = Mathf.Sqrt(gx * gx + gy * gy);
                magnitude = Mathf.Clamp01(magnitude);

                edges.SetPixel(x, y, new Color(magnitude, magnitude, magnitude));
            }
        }

        edges.Apply();
        return edges;
    }

    /// <summary>
    /// Convert color to grayscale value
    /// </summary>
    private float GetGrayscale(Color color)
    {
        return color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
    }

    /// <summary>
    /// Resize a texture to new dimensions
    /// </summary>
    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        if (source.width == newWidth && source.height == newHeight)
        {
            return source;
        }

        Texture2D result = new Texture2D(newWidth, newHeight, source.format, false);

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                float u = x / (float)newWidth;
                float v = y / (float)newHeight;
                Color color = source.GetPixelBilinear(u, v);
                result.SetPixel(x, y, color);
            }
        }

        result.Apply();
        return result;
    }

    /// <summary>
    /// Set the match threshold
    /// </summary>
    public void SetMatchThreshold(float threshold)
    {
        matchThreshold = Mathf.Clamp01(threshold);
    }

    /// <summary>
    /// Get a visual diff of two images (highlight differences)
    /// </summary>
    public Texture2D GetDifferenceMap(Texture2D image1, Texture2D image2)
    {
        Texture2D resized1 = ResizeTexture(image1, image2.width, image2.height);
        Texture2D diffMap = new Texture2D(image2.width, image2.height);

        Color[] pixels1 = resized1.GetPixels();
        Color[] pixels2 = image2.GetPixels();

        for (int i = 0; i < pixels1.Length; i++)
        {
            float diff = Mathf.Abs(pixels1[i].r - pixels2[i].r) +
                        Mathf.Abs(pixels1[i].g - pixels2[i].g) +
                        Mathf.Abs(pixels1[i].b - pixels2[i].b);

            diff /= 3f;

            // Show differences in red
            Color diffColor = new Color(diff, 1f - diff, 1f - diff);
            diffMap.SetPixel(i % image2.width, i / image2.width, diffColor);
        }

        diffMap.Apply();

        if (resized1 != image1)
        {
            Destroy(resized1);
        }

        return diffMap;
    }
}
