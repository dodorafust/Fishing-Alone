using UnityEngine;

public class LiveAnimal : MonoBehaviour
{
    [Header("Dữ liệu Con Mồi")]
    public BaitData baitData; // Kéo file BaitData của con gà, giun... vào đây

    [Header("Đặc tính sinh học")]
    [Tooltip("Tích vào đây nếu là Rắn, Rết... chưa bắt được ngay")]
    public bool isDangerous = false; 

    // Sau này chúng ta sẽ viết thêm AI bỏ chạy hoặc AI cắn người ở đây!
}