using UnityEngine;

public enum ItemType { Fish, Bait, Rod }

[CreateAssetMenu(fileName = "New Item", menuName = "Fishing Game/Basic Item")]
public class ItemData : ScriptableObject
{
    [Header("Thông tin chung")]
    public string itemName = "Tên vật phẩm";
    public Sprite icon; 
    public ItemType itemType; 
    
    [Header("Giá trị kinh tế")]
    public int price = 10;
    public bool isTransmuted = false;

    // ==========================================
    // [THÊM ĐOẠN NÀY]: Thông tin dành cho Shop
    // ==========================================
    [Header("Thông tin Cửa hàng")]
    [TextArea(3, 5)] // Tạo khung nhập chữ to trên Unity (3-5 dòng)
    public string description = "Nhập mô tả sản phẩm vào đây...";
    
    [Tooltip("Cửa hàng phải đạt cấp độ này mới hiện đồ vật này ra")]
    public int requiredShopLevel = 1; 
    // ==========================================

    [Header("Mô hình 3D (Dành riêng cho Cần câu/Cá)")]
    public GameObject modelPrefab;
}