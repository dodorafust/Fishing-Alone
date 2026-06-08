using UnityEngine;

public class FishTrader : MonoBehaviour
{
    public string traderName = "Khu vực Thu mua";
    
    private PlayerWallet playerWallet;
    private HotbarManager hotbarManager;

    void Start()
    {
        playerWallet = FindObjectOfType<PlayerWallet>();
        hotbarManager = FindObjectOfType<HotbarManager>();
    }

    public void SellEquippedFish()
    {
        if (hotbarManager == null || playerWallet == null) return;

        ItemData equippedItem = hotbarManager.GetCurrentEquippedItem();

        if (equippedItem != null && equippedItem is FishData fish)
        {
            playerWallet.AddGold(fish.price);
            Debug.Log($"Đã bán [{fish.itemName}] với giá: {fish.price} Vàng!");
            hotbarManager.ClearEquippedItem(); 
        }
        else
        {
            Debug.Log("Trên tay bạn không cầm con cá nào cả!");
        }
    }
}