using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có để chuyển Scene
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Cài đặt UI")]
    public Toggle fullscreenToggle;

    private void Start()
    {
        // Khi vừa mở game, kiểm tra xem máy tính đang ở chế độ nào để cập nhật cái dấu tích cho đúng
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    // Hàm gọi khi bấm nút TRÒ CHƠI MỚI
    public void NewGame()
    {
        SaveSystem.DeleteSaveFile(); // Xóa sạch dữ liệu cũ
        SceneManager.LoadScene(1);   // Load vào hồ câu
    }

    // Hàm gọi khi bấm nút TIẾP TỤC
    public void ContinueGame()
    {
        SceneManager.LoadScene(1);   // Chỉ load vào hồ câu, hệ thống bên trong sẽ tự lấy file Save
    }

    // Hàm gọi khi bấm nút THOÁT
    public void QuitGame()
    {
        Debug.Log("Đã thoát game!"); // Dòng này để test trong Unity Editor
        Application.Quit(); // Lệnh này chỉ hoạt động khi game đã Build ra file .exe
    }

    // Hàm gọi khi bấm vào dấu tích Toàn màn hình
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log("Chế độ toàn màn hình: " + isFullscreen);
    }
}