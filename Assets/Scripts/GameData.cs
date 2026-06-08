using UnityEngine;
using System.Collections.Generic;

// Dòng [System.Serializable] cực kỳ quan trọng, nó cho phép Unity biến đoạn code này thành dữ liệu để lưu vào máy tính.
[System.Serializable] 
public class GameData
{
    public bool isNewGame; 

    // 1. THỜI GIAN
    public int currentDay; 
    public float currentHour; // Thêm Giờ

    // 2. KINH TẾ & TIẾN ĐỘ
    public int money;      
    public float monsterTributeProgress; // Thanh pin cống phẩm
    public int currentShopLevel;
    public int currentBagLevel;
    public int unlockedFishSlots;
    public int currentSacrificedValue;

    // 3. TỌA ĐỘ
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;

    // 4. KHO ĐỒ (Lưu tên hoặc ID của vật phẩm thay vì lưu cả mô hình 3D)
    public List<string> fishInventory;
    public List<string> baitInventory;
    public List<string> rodInventory;

    // Dòng này là "Giá trị mặc định" nếu người chơi mới tải game lần đầu (New Game)
    public GameData()
    {
        isNewGame = true;
        currentDay = 1;
        currentHour = 6f; // Mặc định 6h sáng
        money = 100;
        monsterTributeProgress = 0f;
        currentShopLevel = 1;
        currentBagLevel = 0;
        unlockedFishSlots = 6;
        currentSacrificedValue = 0;
        playerPosX = 301.682f;
        playerPosY = 9.925f; 
        playerPosZ = 160.464f;

        // Khởi tạo túi đồ trống
        fishInventory = new List<string>();
        baitInventory = new List<string>();
        rodInventory = new List<string>();
    }
}