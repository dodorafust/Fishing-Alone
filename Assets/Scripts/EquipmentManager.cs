using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Liên kết UI")]
    public InventorySlot[] rodSlots; 

    [Header("Vị trí cầm đồ")]
    [Tooltip("Kéo vật thể RightHand vào đây")]
    public Transform rightHand; 

    private ItemData currentEquippedData;     // Ghi nhớ dữ liệu cần câu đang cầm
    private GameObject currentEquippedModel;  // Ghi nhớ mô hình 3D đang cầm để sau này xóa đi

    void Update()
    {
        ItemData foundRodData = null;

        // Đi tuần tra các ô chứa cần câu
        foreach (InventorySlot slot in rodSlots)
        {
            DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
            if (item != null && item.itemData.itemType == ItemType.Rod)
            {
                foundRodData = item.itemData;
                break; // Tìm thấy cần câu thì dừng lại
            }
        }

        // NẾU PHÁT HIỆN SỰ THAY ĐỔI (Trang bị cần mới, hoặc cất cần đi)
        if (foundRodData != currentEquippedData)
        {
            // 1. Xóa mô hình 3D cũ trên tay (nếu có)
            if (currentEquippedModel != null)
            {
                Destroy(currentEquippedModel);
            }

            // 2. Cập nhật dữ liệu cần câu mới
            currentEquippedData = foundRodData;

            // 3. Nếu có cần câu mới -> Triệu hồi (Spawn) mô hình 3D của nó lên tay
            if (currentEquippedData != null && currentEquippedData.modelPrefab != null)
            {
                // Instantiate sẽ tạo ra vật thể mới và gán nó làm con của rightHand
                currentEquippedModel = Instantiate(currentEquippedData.modelPrefab, rightHand, false);
            }
        }
    }
}