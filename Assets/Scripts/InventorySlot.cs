using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [Header("Phân loại Ô chứa")]
    [Tooltip("Chọn loại vật phẩm được phép thả vào ô này (Ví dụ: Fish, Bait, Rod)")]
    public ItemType allowedType; 

    [Header("Trạng thái Ô chứa")]
    public bool isLocked = false; // Tích vào nếu muốn ô này bị khóa ban đầu
    public GameObject lockIcon;   // Gắn hình ảnh ổ khóa vào đây

    private void Start()
    {
        // Khi chạy game, nếu ô bị khóa thì bật hình ổ khóa lên, ngược lại thì tắt đi
        if (lockIcon != null)
        {
            lockIcon.SetActive(isLocked);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 1. ĐANG KHÓA -> TỪ CHỐI NHẬN ĐỒ
        if (isLocked) return; 

        // Lấy thông tin vật phẩm đang được kéo
        GameObject dropped = eventData.pointerDrag;
        DraggableItem draggedItem = dropped.GetComponent<DraggableItem>();

        // 2. KIỂM TRA ĐÚNG LOẠI VẬT PHẨM THÌ MỚI CHO THẢ VÀO
        if (draggedItem != null && draggedItem.itemData.itemType == allowedType)
        {
            // 3. TÌM XEM TRONG Ô ĐÃ CÓ ĐỒ CHƯA (Dùng GetComponent để tự động bỏ qua cái LockIcon)
            DraggableItem itemInSlot = GetComponentInChildren<DraggableItem>();

            // 4. NẾU Ô TRỐNG ĐỒ -> NHẬN ĐỒ MỚI VÀO
            if (itemInSlot == null)
            {
                draggedItem.parentAfterDrag = transform;
            }
        }
    }

    // Sau này Cửa hàng sẽ gọi hàm này để mở khóa ô
    public void UnlockSlot()
    {
        isLocked = false;
        if (lockIcon != null) lockIcon.SetActive(false);
    }
}