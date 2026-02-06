using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
    [Tooltip("The sleeping mask sprite (shown during cooldown)")]
    [SerializeField] private Sprite maskAsleepSprite;

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownDuration = 20f;
    [SerializeField] private float wakeTransitionStart = 17f; // When sprite starts transitioning
    [SerializeField] private float wakeTransitionDuration = 3f; // How long the transition takes

    [Header("Particle Settings")]
    [SerializeField] private float particleSpawnRadius = 2f; // Radius around mask to spawn particles
    [SerializeField] private float particleSpeed = 1f; // Speed particles move toward center
    [SerializeField] private Color particleColor = new Color(1f, 1f, 0.3f, 1f); // Yellow particles
    [SerializeField] private float particleSize = 0.1f;
    [SerializeField] private float maxParticlesPerSecond = 30f; // Max spawn rate near wake time

    [Header("Settings")]
    [SerializeField] private Color yellowFilterColor = new Color(1f, 0.9f, 0.3f, 0.3f);
    [SerializeField] private LayerMask clickableLayer = -1; // All layers by default

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer awakeSpriteRenderer; // For crossfade effect
    private bool isMaskActive = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private Camera mainCamera;
    private Collider2D maskCollider;

    // Particle system
    private List<GlowParticle> activeParticles = new List<GlowParticle>();
    private float particleSpawnTimer = 0f;
    private GameObject particleContainer;

    // Particle class
    private class GlowParticle
    {
        public GameObject gameObject;
        public SpriteRenderer renderer;
        public Vector2 startPosition;
        public float lifetime;
        public float maxLifetime;

        public GlowParticle(GameObject go, SpriteRenderer sr, Vector2 startPos, float maxLife)
        {
            gameObject = go;
            renderer = sr;
            startPosition = startPos;
            lifetime = 0f;
            maxLifetime = maxLife;
        }
    }

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

        // Create particle container
        CreateParticleContainer();

        // Create awake sprite renderer for crossfade
        CreateAwakeSpriteRenderer();

        // Start awake and ready (not on cooldown)
        isMaskActive = false;
        isOnCooldown = false;

        // Hide yellow filter initially
        if (yellowFilterImage != null)
        {
            yellowFilterImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Create a second sprite renderer for crossfade effect
    /// </summary>
    private void CreateAwakeSpriteRenderer()
    {
        GameObject awakeObj = new GameObject("AwakeMaskLayer");
        awakeObj.transform.SetParent(transform, false);
        awakeObj.transform.localPosition = Vector3.zero;

        awakeSpriteRenderer = awakeObj.AddComponent<SpriteRenderer>();
        awakeSpriteRenderer.sprite = maskSprite;
        awakeSpriteRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        awakeSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // Above sleeping sprite
        awakeSpriteRenderer.color = new Color(1f, 1f, 1f, 0f); // Start transparent

        awakeObj.SetActive(false);
    }

    /// <summary>
    /// Create container for particles
    /// </summary>
    private void CreateParticleContainer()
    {
        particleContainer = new GameObject("ParticleContainer");
        particleContainer.transform.SetParent(transform, false);
        particleContainer.transform.localPosition = Vector3.zero;
        particleContainer.SetActive(false);
    }

    private void Update()
    {
        // Handle cooldown timer
        if (isOnCooldown)
        {
            cooldownTimer += Time.deltaTime;

            // Update particles during entire sleep duration
            UpdateParticles();

            // Spawn particles throughout cooldown
            SpawnParticles();

            // Update sprite crossfade transition
            UpdateWakeTransition();

            // End cooldown at 20 seconds
            if (cooldownTimer >= cooldownDuration)
            {
                EndCooldown();
            }

            return; // Don't check for clicks during cooldown
        }

        // Check for click using new Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckForClick();
        }
    }

    /// <summary>
    /// Update the wake transition (crossfade from sleeping to awake sprite)
    /// </summary>
    private void UpdateWakeTransition()
    {
        if (cooldownTimer < wakeTransitionStart)
        {
            // Not started yet - ensure awake sprite is hidden
            if (awakeSpriteRenderer != null && awakeSpriteRenderer.gameObject.activeSelf)
            {
                awakeSpriteRenderer.gameObject.SetActive(false);
            }
            return;
        }

        // Enable awake sprite renderer if not active
        if (awakeSpriteRenderer != null && !awakeSpriteRenderer.gameObject.activeSelf)
        {
            awakeSpriteRenderer.gameObject.SetActive(true);
            Debug.Log("Wake transition started - crossfading to awake sprite");
        }

        // Calculate transition progress (0 to 1 over 3 seconds)
        float transitionElapsed = cooldownTimer - wakeTransitionStart;
        float transitionProgress = Mathf.Clamp01(transitionElapsed / wakeTransitionDuration);

        // Fade in awake sprite
        if (awakeSpriteRenderer != null)
        {
            Color awakeColor = awakeSpriteRenderer.color;
            awakeColor.a = transitionProgress;
            awakeSpriteRenderer.color = awakeColor;
        }

        // Optional: Fade out sleeping sprite (if you want)
        // Uncomment below if you want the sleeping sprite to fade out as awake fades in
        /*
        if (spriteRenderer != null)
        {
            Color sleepColor = spriteRenderer.color;
            sleepColor.a = 1f - transitionProgress;
            spriteRenderer.color = sleepColor;
        }
        */
    }

    /// <summary>
    /// Spawn particles based on cooldown progress
    /// </summary>
    private void SpawnParticles()
    {
        // Calculate spawn rate based on cooldown progress
        float cooldownProgress = cooldownTimer / cooldownDuration;

        // Start with a few particles, increase as we get closer to wake time
        float particlesPerSecond = Mathf.Lerp(2f, maxParticlesPerSecond, cooldownProgress);

        particleSpawnTimer += Time.deltaTime;
        float spawnInterval = 1f / particlesPerSecond;

        while (particleSpawnTimer >= spawnInterval)
        {
            particleSpawnTimer -= spawnInterval;
            CreateParticle();
        }
    }

    /// <summary>
    /// Create a single particle at a random position around the mask
    /// </summary>
    private void CreateParticle()
    {
        // Random angle around the mask
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Random radius within spawn radius
        float distance = Random.Range(particleSpawnRadius * 0.8f, particleSpawnRadius);

        Vector2 spawnOffset = new Vector2(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance
        );

        // Create particle GameObject
        GameObject particleObj = new GameObject("Particle");
        particleObj.transform.SetParent(particleContainer.transform, false);
        particleObj.transform.localPosition = spawnOffset;

        // Add sprite renderer for particle
        SpriteRenderer particleRenderer = particleObj.AddComponent<SpriteRenderer>();

        // Create a simple circle sprite (or use a small texture)
        Texture2D circleTexture = CreateCircleTexture(32);
        Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);

        particleRenderer.sprite = circleSprite;
        particleRenderer.color = particleColor;
        particleRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        particleRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;

        // Set particle size
        particleObj.transform.localScale = Vector3.one * particleSize;

        // Random lifetime (1-3 seconds)
        float lifetime = Random.Range(1f, 3f);

        // Add to active particles list
        GlowParticle particle = new GlowParticle(particleObj, particleRenderer, spawnOffset, lifetime);
        activeParticles.Add(particle);
    }

    /// <summary>
    /// Create a simple circle texture for particles
    /// </summary>
    private Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (distance / radius));

                // Soft edge
                alpha = Mathf.Pow(alpha, 2f);

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Update all active particles
    /// </summary>
    private void UpdateParticles()
    {
        Vector2 maskCenter = transform.position;

        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            GlowParticle particle = activeParticles[i];
            particle.lifetime += Time.deltaTime;

            // Calculate progress (0 to 1)
            float progress = particle.lifetime / particle.maxLifetime;

            if (progress >= 1f)
            {
                // Particle reached center, destroy it
                Destroy(particle.gameObject);
                activeParticles.RemoveAt(i);
                continue;
            }

            // Move particle toward mask center
            Vector2 currentPos = particle.gameObject.transform.position;
            Vector2 targetPos = maskCenter;

            // Use smooth ease-in curve
            float easedProgress = Mathf.Pow(progress, 0.5f);
            Vector2 newPos = Vector2.Lerp(particle.startPosition + (Vector2)transform.position, targetPos, easedProgress);

            particle.gameObject.transform.position = newPos;

            // Fade out as particle gets closer
            Color color = particle.renderer.color;
            color.a = Mathf.Lerp(1f, 0f, progress);
            particle.renderer.color = color;

            // Shrink particle as it approaches center
            float scale = Mathf.Lerp(particleSize, particleSize * 0.3f, progress);
            particle.gameObject.transform.localScale = Vector3.one * scale;
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
    /// Deactivate mask mode: hide yellow filter and start cooldown
    /// </summary>
    public void DeactivateMask()
    {
        isMaskActive = false;

        // Hide yellow filter
        if (yellowFilterImage != null)
        {
            yellowFilterImage.gameObject.SetActive(false);
        }

        // Start cooldown - show sleeping mask
        StartCooldown();

        Debug.Log("Mask deactivated - starting cooldown");
    }

    /// <summary>
    /// Start the cooldown period with sleeping mask
    /// </summary>
    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = 0f;
        particleSpawnTimer = 0f;

        // Change to sleeping mask sprite
        if (spriteRenderer != null && maskAsleepSprite != null)
        {
            spriteRenderer.sprite = maskAsleepSprite;
            spriteRenderer.color = Color.white; // Full opacity
        }

        // Reset awake sprite renderer (will be faded in during transition)
        if (awakeSpriteRenderer != null)
        {
            awakeSpriteRenderer.color = new Color(1f, 1f, 1f, 0f); // Transparent
            awakeSpriteRenderer.gameObject.SetActive(false);
        }

        // Enable particle container
        if (particleContainer != null)
        {
            particleContainer.SetActive(true);
        }

        Debug.Log("Mask is sleeping - particles starting...");
    }

    /// <summary>
    /// End the cooldown and wake up the mask
    /// </summary>
    private void EndCooldown()
    {
        isOnCooldown = false;
        cooldownTimer = 0f;

        // Switch to awake sprite (crossfade should be complete by now)
        if (spriteRenderer != null && maskSprite != null)
        {
            spriteRenderer.sprite = maskSprite;
            spriteRenderer.color = Color.white;
        }

        // Hide the awake layer (no longer needed)
        if (awakeSpriteRenderer != null)
        {
            awakeSpriteRenderer.gameObject.SetActive(false);
            awakeSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);
        }

        // Disable and clear particles
        if (particleContainer != null)
        {
            particleContainer.SetActive(false);
        }

        // Clear all active particles
        foreach (var particle in activeParticles)
        {
            if (particle.gameObject != null)
            {
                Destroy(particle.gameObject);
            }
        }
        activeParticles.Clear();

        Debug.Log("Mask is awake and ready!");
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
