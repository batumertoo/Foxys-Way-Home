using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Level1"; // First level scene name
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Main menu scene name

    [SerializeField] private GameObject creditsPanel; // Credits panel to show/hide
    [SerializeField] private GameObject controlsPanel; // Controls panel to show/hide
    [SerializeField] private GameObject pausePanel; // Pause panel for in-game pausing
    [SerializeField] private GameObject levelCompletePanel; // Level complete panel
    [SerializeField] private string creditsSceneName; // Optional: Separate credits scene

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel; // Main menu panel

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button backButton; // Back button on credits panel
    [SerializeField] private Button controlsBackButton; // Back button on controls panel
    [SerializeField] private Button resumeButton; // Resume button on pause menu
    [SerializeField] private Button restartButton; // Restart button on pause menu
    [SerializeField] private Button mainMenuButton; // Return to main menu button on pause menu
    [SerializeField] private Button pauseExitButton; // Exit button on pause menu
    [SerializeField] private Button nextLevelButton; // Button to go to next level

    private bool isPaused = false;
    private string nextLevelName; // Stores the next level name when level is complete

    public static MenuManager Instance;

    private void Awake()
    {
        // Singleton pattern with special handling:
        // - Gameplay MenuManager (has pausePanel) persists across levels
        // - When loading Main Menu, let the scene's MenuManager replace the persisted one
        if (Instance != null && Instance != this)
        {
            bool thisIsMainMenuManager = (mainMenuPanel != null) && (pausePanel == null);
            bool existingIsGameplayManager = (Instance != null) && (Instance.pausePanel != null);

            if (thisIsMainMenuManager && existingIsGameplayManager)
            {
                // Replace persisted gameplay manager with the main menu manager
                Destroy(Instance.gameObject);
                Instance = this;
            }
            else
            {
                // Keep existing instance; discard duplicate
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            Instance = this;
        }

        // Persist only for gameplay scenes (has pausePanel)
        if (pausePanel != null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // Auto-assign button listeners for main menu
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(ShowControls);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (backButton != null)
            backButton.onClick.AddListener(HideCredits);

        if (controlsBackButton != null)
            controlsBackButton.onClick.AddListener(HideControls);

        // Auto-assign button listeners for pause menu
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (pauseExitButton != null)
            pauseExitButton.onClick.AddListener(ExitGame);

        // Auto-assign button listener for level complete
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);

        // Hide panels at start
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    private void Update()
    {
        // Handle Escape key for different states
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If other modal panels are open (Shop or Inventory), close them first and do nothing else
            var shop = FindObjectOfType<ShopManager>();
            if (shop != null && shop.IsOpen)
            {
                shop.CloseShop();
                return;
            }
            if (InventoryManager.Instance != null && InventoryManager.Instance.IsOpen)
            {
                InventoryManager.Instance.CloseInventory();
                return;
            }

            // If in Credits or Controls during pause, go back to pause menu
            if (isPaused && ((creditsPanel != null && creditsPanel.activeSelf) || (controlsPanel != null && controlsPanel.activeSelf)))
            {
                if (creditsPanel != null && creditsPanel.activeSelf)
                    HideCredits();
                else if (controlsPanel != null && controlsPanel.activeSelf)
                    HideControls();
                return;
            }

            // Pause/Resume with Escape key during gameplay (only if pausePanel is assigned)
            if (pausePanel != null)
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }
    }

    // ===== MAIN MENU FUNCTIONS =====

    public void PlayGame()
    {
        ForceUnpause();
        if (PermanentUI.perm != null)
            PermanentUI.perm.Reset();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowCredits()
    {
        if (!string.IsNullOrEmpty(creditsSceneName))
        {
            SceneManager.LoadScene(creditsSceneName);
        }
        else if (creditsPanel != null)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
            if (pausePanel != null)
                pausePanel.SetActive(false);
            creditsPanel.SetActive(true);
        }
    }

    public void HideCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        // Return to pause menu if paused, otherwise return to main menu
        if (isPaused)
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }
        else
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }
    }

    public void ShowControls()
    {
        if (controlsPanel != null)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
            if (pausePanel != null)
                pausePanel.SetActive(false);
            controlsPanel.SetActive(true);
        }
    }

    public void HideControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // Return to pause menu if paused, otherwise return to main menu
        if (isPaused)
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }
        else
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }
    }

    // ===== PAUSE MENU FUNCTIONS =====

    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null)
            pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        ForceUnpause();
    }

    private void ForceUnpause()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    public void Restart()
    {
        ForceUnpause();
        if (PermanentUI.perm != null)
            PermanentUI.perm.RestoreCoinsToLevelStart();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        ForceUnpause();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        ForceUnpause();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ===== LEVEL COMPLETE FUNCTIONS =====

    public void ShowLevelComplete(string nextLevel)
    {
        nextLevelName = nextLevel;
        isPaused = true;
        Time.timeScale = 0f; // Pause the game

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
        else
            LoadNextLevel(); // If no panel, just load next level immediately
    }

    public void LoadNextLevel()
    {
        ForceUnpause();
        // Don't reset permanent UI here usually, as we want to keep coins between levels
        // If you want to reset, uncomment:
        // if (PermanentUI.perm != null) PermanentUI.perm.Reset();

        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogWarning("Next level name is empty!");
            GoToMainMenu();
        }
    }
}