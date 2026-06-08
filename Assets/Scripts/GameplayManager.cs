using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    void Start()
    {
        // Vừa vào game là lập tức tải dữ liệu ngay
        LoadSavedData();
    }

    private void LoadSavedData()
    {
        GameData data = SaveSystem.LoadGame();

        // 1. TRẢ LẠI TỌA ĐỘ
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && !data.isNewGame) 
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
            if (cc != null) cc.enabled = true;
        }

        // 2. TRẢ LẠI THỜI GIAN
        TimeManager timeScript = FindDisabledOrActiveObjectOfType<TimeManager>();
        if (timeScript != null)
        {
            timeScript.LoadTimeState(data.currentDay, data.currentHour, data.currentSacrificedValue);
            // timeScript.UpdateUI(); // Nhớ gọi hàm cập nhật chữ trên màn hình nhé
        }

        // 3. TRẢ LẠI TIỀN & CỐNG PHẨM
        PlayerWallet wallet = FindDisabledOrActiveObjectOfType<PlayerWallet>();
        if (wallet != null)
        {
            wallet.SetGold(data.money);
        }
        else
        {
            GameManager gmScript = FindObjectOfType<GameManager>();
            if (gmScript != null)
            {
                gmScript.money = data.money;
                gmScript.tributeProgress = data.monsterTributeProgress;
                // gmScript.UpdateMoneyUI(); 
                // gmScript.UpdateTributeBar(); // Cập nhật lại thanh UI
            }
        }

        ShopUIController shopController = FindDisabledOrActiveObjectOfType<ShopUIController>();
        if (shopController != null)
        {
            shopController.currentShopLevel = data.currentShopLevel;
            shopController.currentBagLevel = data.currentBagLevel;
        }

        HotbarManager hotbarManager = FindDisabledOrActiveObjectOfType<HotbarManager>();
        if (hotbarManager != null)
        {
            hotbarManager.unlockedFishSlots = data.unlockedFishSlots;
            hotbarManager.UpdateUnlockedSlotsUI();

            // 4. TRẢ LẠI KHO ĐỒ
            if (!data.isNewGame)
            {
                hotbarManager.LoadInventoryFromNames(data.fishInventory, data.baitInventory, data.rodInventory);
            }
        }
    }

    private T FindDisabledOrActiveObjectOfType<T>() where T : Object
    {
        T result = FindObjectOfType<T>();
        if (result == null)
        {
            T[] allObjects = Resources.FindObjectsOfTypeAll<T>();
            if (allObjects != null && allObjects.Length > 0)
            {
                result = allObjects[0];
            }
        }
        return result;
    }
}