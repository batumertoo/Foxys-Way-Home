using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Button inventoryButton; 
    [SerializeField] private Button closeButton; 

    [Header("Items UI")]
    // Use Buttons
    [SerializeField] private Button btnMagnetUse;
    [SerializeField] private Button btnDoubleJumpUse;
    [SerializeField] private Button btnDashUse;
    
    // Counts Text
    [SerializeField] private TextMeshProUGUI txtMagnetCount;
    [SerializeField] private TextMeshProUGUI txtDoubleJumpCount; 
    [SerializeField] private TextMeshProUGUI txtDashCount; 

    [Header("Item Counts")]
    public int magnetCount = 0;
    public int doubleJumpCount = 0; 
    public int dashCount = 0; 

    // Permanent unlocks
    public bool doubleJumpUnlocked = false;
    public bool dashUnlocked = false;

    private bool isOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(ToggleInventory);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseInventory);

        if (btnMagnetUse != null)
            btnMagnetUse.onClick.AddListener(UseMagnet);
            
        if (btnDoubleJumpUse != null)
            btnDoubleJumpUse.onClick.AddListener(UseDoubleJump);
            
        if (btnDashUse != null)
            btnDashUse.onClick.AddListener(UseDash);

        UpdateUI();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isOpen = false;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        UpdateUI();
    }

    public bool IsOpen => isOpen && inventoryPanel != null && inventoryPanel.activeSelf;

    public bool DoubleJumpUnlocked => doubleJumpUnlocked;
    public bool DashUnlocked => dashUnlocked;

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        
        // Clear focus to avoid spacebar re-triggering
        if(UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            
        UpdateUI();
    }

    public void CloseInventory()
    {
        isOpen = false;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            UpdateUI();
        }
    }

    public void AddItem(string itemName, int amount)
    {
        if (itemName == "Magnet")
        {
            magnetCount += amount;
        }
        else if (itemName == "DoubleJump")
        {
            UnlockDoubleJump();
        }
        else if (itemName == "Dash")
        {
            UnlockDash();
        }
        
        UpdateUI();
    }

    // ===== Use Logic (Consumable -> Duration) =====

    public void UseMagnet()
    {
        if (magnetCount > 0)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.ActivateMagnet(10f); 
                magnetCount--;
                Debug.Log("Magnet activated!");
                UpdateUI();
            }
        }
    }

    public void UseDoubleJump()
    {
        // No activation here; unlock is permanent and handled on purchase
    }

    public void UseDash()
    {
        // No activation here; unlock is permanent and handled on purchase
    }

    public void UnlockDoubleJump()
    {
        if (doubleJumpUnlocked) return;
        doubleJumpUnlocked = true;
        doubleJumpCount = 1; // For display only

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.EnablePermanentDoubleJump();
        }
        UpdateUI();
    }

    public void UnlockDash()
    {
        if (dashUnlocked) return;
        dashUnlocked = true;
        dashCount = 1; // For display only

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.EnablePermanentDash();
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (txtMagnetCount != null) txtMagnetCount.text = "x " + magnetCount.ToString();
        if (btnMagnetUse != null) btnMagnetUse.interactable = (magnetCount > 0);

        if (txtDoubleJumpCount != null) txtDoubleJumpCount.text = doubleJumpUnlocked ? "Unlocked" : "Locked";
        if (btnDoubleJumpUse != null) btnDoubleJumpUse.interactable = false; // Not activated here
            
        if (txtDashCount != null) txtDashCount.text = dashUnlocked ? "Unlocked" : "Locked";
        if (btnDashUse != null) btnDashUse.interactable = false; // Not activated here
    }
}
