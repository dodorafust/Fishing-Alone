using UnityEngine;

// Dòng này giúp tạo menu chuột phải giống y hệt Rod Data và Fish Data của bạn
[CreateAssetMenu(fileName = "New Bait", menuName = "Fishing Game/Bait Data")]
public class BaitData : ItemData // Kế thừa từ ItemData để dùng chung được với kho đồ UI
{
    [Header("Chỉ số Mồi câu")]
    [Tooltip("Chỉ số nhử: Càng cao càng dễ gọi được cá hiếm/thủy quái tới cắn câu")]
    public float attractionPower = 10f;

    [Tooltip("Thời gian chờ cắn câu (giây) - Mồi xịn thì cá cắn nhanh hơn")]
    public float biteTimeReduction = 2f;
    
    // Bạn có thể thêm bất kỳ chỉ số nào bạn muốn ở đây sau này (Ví dụ: Độ bền của mồi, Loại cá yêu thích...)
    
    private void OnEnable()
    {
        // Tự động gán loại vật phẩm là Bait để hệ thống nhận diện đúng
        itemType = ItemType.Bait;
    }
}