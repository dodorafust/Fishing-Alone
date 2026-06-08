using UnityEngine;

public class ReadableItem : MonoBehaviour
{
    [Header("Cài đặt Giao diện")]
    public string promptMessage = "Lá thư"; // Chữ hiện ra khi lại gần
    
    [Header("Nội dung tài liệu")]
    public Sprite documentImage; // Kéo tấm ảnh nền (lá thư ố vàng / trang sách) vào đây
    
    [TextArea(10, 20)] // Tạo khung nhập chữ to trong Inspector để bạn dễ viết cốt truyện
    public string documentContent; 
}