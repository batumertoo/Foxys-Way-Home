using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    // Singleton Instance
    public static ShopManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button btnOpenShop; 
    [SerializeField] private Button btnCloseShop;
    
    [Header("Shop Items")]
    [SerializeField] private Button btnBuyMagnet;
    [SerializeField] private Button btnBuyDoubleJump; // New
    [SerializeField] private Button btnBuyDash; // New

    [Header("Price Labels")]
    [SerializeField] private TextMeshProUGUI txtMagnetPrice;
    [SerializeField] private TextMeshProUGUI txtDoubleJumpPrice;
    [SerializeField] private TextMeshProUGUI txtDashPrice;
    
    [SerializeField] private TextMeshProUGUI txtCoinDisplay; 
    [SerializeField] private TextMeshProUGUI txtFeedback; 
    
    [Header("Prices")]
    [SerializeField] private int magnetPrice = 10;
    [SerializeField] private int doubleJumpPrice = 10;
    [SerializeField] private int dashPrice = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);

        if (btnOpenShop != null)
            btnOpenShop.onClick.AddListener(OpenShop);
            
        if (btnCloseShop != null)
            btnCloseShop.onClick.AddListener(CloseShop);

        if (btnBuyMagnet != null)
            btnBuyMagnet.onClick.AddListener(BuyMagnet);
            
        if (btnBuyDoubleJump != null)
            btnBuyDoubleJump.onClick.AddListener(BuyDoubleJump);
            
        if (btnBuyDash != null)
            btnBuyDash.onClick.AddListener(BuyDash);

        if (txtFeedback != null)
            txtFeedback.text = ""; 

        UpdatePriceTexts();
    }

    private void Update()
    {
        if (shopPanel != null && shopPanel.activeSelf && txtCoinDisplay != null)
        {
            if (PermanentUI.perm != null)
                txtCoinDisplay.text = "Coins: " + PermanentUI.perm.coins.ToString();
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }
    
    public bool IsOpen => shopPanel != null && shopPanel.activeSelf;

    public void BuyMagnet()
    {
        BuyItem("Magnet", magnetPrice);
    }

    public void BuyDoubleJump()
    {
        BuyItem("DoubleJump", doubleJumpPrice);
    }

    public void BuyDash()
    {
        BuyItem("Dash", dashPrice);
    }

    private void BuyItem(string itemName, int price)
    {
        if (PermanentUI.perm != null && InventoryManager.Instance != null)
        {
            if (itemName == "DoubleJump" && InventoryManager.Instance.DoubleJumpUnlocked)
            {
                ShowFeedback("Already owned!", Color.yellow);
                return;
            }

            if (itemName == "Dash" && InventoryManager.Instance.DashUnlocked)
            {
                ShowFeedback("Already owned!", Color.yellow);
                return;
            }

            if (PermanentUI.perm.coins >= price)
            {
                // Deduct coins
                PermanentUI.perm.coins -= price;
                if (PermanentUI.perm.coinText != null)
                    PermanentUI.perm.coinText.text = PermanentUI.perm.coins.ToString();
                
                // Add item or unlock permanently
                InventoryManager.Instance.AddItem(itemName, 1);
                
                Debug.Log($"Purchased {itemName}!");
                ShowFeedback("Purchased!", Color.green);
            }
            else
            {
                Debug.Log("Not enough coins!");
                ShowFeedback("Not enough coins!", Color.red);
            }
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        if (txtFeedback != null)
        {
            txtFeedback.text = message;
            txtFeedback.color = color;
            StopAllCoroutines(); 
            StartCoroutine(ClearFeedback());
        }
    }

    private IEnumerator ClearFeedback()
    {
        yield return new WaitForSeconds(2f);
        if (txtFeedback != null)
            txtFeedback.text = "";
    }

    private void UpdatePriceTexts()
    {
        if (txtMagnetPrice != null)
            txtMagnetPrice.text = $"{magnetPrice} coins";
        if (txtDoubleJumpPrice != null)
            txtDoubleJumpPrice.text = $"{doubleJumpPrice} coins";
        if (txtDashPrice != null)
            txtDashPrice.text = $"{dashPrice} coins";
    }
}
