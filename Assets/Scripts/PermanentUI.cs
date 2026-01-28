using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PermanentUI : MonoBehaviour
{
    public static PermanentUI perm;
    
    public int coins = 0;
    public Text coinText;
    // Baseline coins saved at the start of each level
    public int levelStartCoins = 0;
    
    private void Start()
    {
        if (!perm)
        {
            perm = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Initialize coin text
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }

        // If Start ran in first scene, initialize baseline
        levelStartCoins = coins;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Save baseline coins for this level
        levelStartCoins = coins;
    }
    
    public void Reset()
    {
        coins = 0;
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }

    public void RestoreCoinsToLevelStart()
    {
        coins = levelStartCoins;
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }
}

