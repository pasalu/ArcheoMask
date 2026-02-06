using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the main menu UI and navigation
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainGameSceneName = "MainGameScene";

    [Header("Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings")]
    [SerializeField] private bool checkForSaveData = true;

    private SceneTransition sceneTransition;

    private void Awake()
    {
        // Get or create scene transition component
        sceneTransition = FindObjectOfType<SceneTransition>();
        if (sceneTransition == null)
        {
            GameObject transitionObj = new GameObject("SceneTransition");
            sceneTransition = transitionObj.AddComponent<SceneTransition>();
            Debug.Log("SceneTransition created automatically");
        }

        // Set up button listeners
        SetupButtons();

        // Check if continue should be enabled
        UpdateContinueButton();

        // Ensure settings panel is hidden initially
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void SetupButtons()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void OnNewGameClicked()
    {
        Debug.Log("New Game clicked");

        // Clear any existing save data
        ClearSaveData();

        // Load main game scene
        LoadMainGameScene();
    }

    /// <summary>
    /// Continue from last save
    /// </summary>
    public void OnContinueClicked()
    {
        Debug.Log("Continue clicked");

        if (HasSaveData())
        {
            // Load main game scene (it will load save data)
            LoadMainGameScene();
        }
        else
        {
            Debug.LogWarning("No save data found!");
        }
    }

    /// <summary>
    /// Open settings menu
    /// </summary>
    public void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Close settings and return to main menu
    /// </summary>
    public void OnSettingsBackClicked()
    {
        Debug.Log("Settings back clicked");

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Load the main game scene with transition
    /// </summary>
    private void LoadMainGameScene()
    {
        Debug.Log($"LoadMainGameScene called - Scene name: {mainGameSceneName}");

        if (sceneTransition != null)
        {
            Debug.Log("SceneTransition found - starting transition");
            sceneTransition.TransitionToScene(mainGameSceneName);
        }
        else
        {
            Debug.LogWarning("SceneTransition is NULL - using direct load");
            // Fallback: direct load
            SceneManager.LoadScene(mainGameSceneName);
        }
    }

    /// <summary>
    /// Check if save data exists
    /// </summary>
    private bool HasSaveData()
    {
        if (!checkForSaveData)
        {
            return false;
        }

        // Check for save data in PlayerPrefs
        return PlayerPrefs.HasKey("GameSaveExists");
    }

    /// <summary>
    /// Update continue button based on save data
    /// </summary>
    private void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            bool hasSave = HasSaveData();
            continueButton.interactable = hasSave;

            // Optional: Change button appearance
            if (!hasSave)
            {
                var colors = continueButton.colors;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                continueButton.colors = colors;
            }
        }
    }

    /// <summary>
    /// Clear save data (when starting new game)
    /// </summary>
    private void ClearSaveData()
    {
        PlayerPrefs.DeleteKey("GameSaveExists");
        PlayerPrefs.Save();
        Debug.Log("Save data cleared");
    }

    /// <summary>
    /// Public method to mark that save data exists (call from game scene)
    /// </summary>
    public static void MarkSaveDataExists()
    {
        PlayerPrefs.SetInt("GameSaveExists", 1);
        PlayerPrefs.Save();
    }
}
