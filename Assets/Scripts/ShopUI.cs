using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public GameObject shopPanel;
    public PlayerWallet wallet;
    public HotbarManager hotbar;
    public BaitData baitItem; // Kéo file mồi của bạn vào đây trong Inspector

    // Gọi hàm này khi nhấn nút E (được gọi từ PlayerInteraction)
    public void OpenShop() { shopPanel.SetActive(true); Cursor.lockState = CursorLockMode.None; }
    
    public void CloseShop() { shopPanel.SetActive(false); Cursor.lockState = CursorLockMode.Locked; }

    public void BuyBait()
    {
        if (wallet.SpendGold(50)) // Giá 50
        {
            hotbar.AddItemToInventory(baitItem);
        }
    }

    public void UpgradeBag()
    {
        if (wallet.SpendGold(500)) // Giá 500
        {
            hotbar.UnlockMoreSlots(1);
        }
    }
}