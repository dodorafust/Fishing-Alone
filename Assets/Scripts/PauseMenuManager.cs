using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("Giao diện UI")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Cài đặt UI Sliders")]
    public Slider audioSlider;
    public Slider mouseSlider;

    private void Start()
    {
        // Khi bắt đầu game, lấy lại dữ liệu cài đặt cũ mà người chơi đã lưu (nếu có)
        // Mặc định âm lượng là 1f (100%), độ nhạy chuột mặc định là 100f
        if (audioSlider != null) audioSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        if (mouseSlider != null) mouseSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 100f);
    }

    private void Update()
    {
        // Bấm Esc để bật/tắt Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // ==========================================
    // CHỨC NĂNG TẠM DỪNG / TIẾP TỤC
    // ==========================================
    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false); // Chắc chắn tắt cả bảng setting
        Time.timeScale = 1f; // Chạy lại thời gian
        GameIsPaused = false;

        // Khóa và ẩn chuột đi để tiếp tục quay camera
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Pause()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Đóng băng thời gian
        GameIsPaused = true;

        // Mở khóa và hiện chuột để bấm UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ==========================================
    // CHUYỂN BẢNG CÀI ĐẶT
    // ==========================================
    public void OpenSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    // ==========================================
    // LƯU CÀI ĐẶT
    // ==========================================
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; // Đổi âm lượng tổng của cả game
        PlayerPrefs.SetFloat("Volume", volume); // Lưu vào ổ cứng
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity); // Lưu vào ổ cứng
    }

    // ==========================================
    // LƯU VÀ THOÁT
    // ==========================================
    public void QuitToMainMenu()
    {
        // 1. Khôi phục thời gian bình thường cho game (vì lúc Pause thời gian đang bị đóng băng)
        Time.timeScale = 1f; 

        // 2. TẠO RA 1 CÁI HỘP RỖNG ĐỂ CHỨA DỮ LIỆU
        GameData dataToSave = new GameData();
        dataToSave.isNewGame = false; // Đánh dấu đây không phải là game mới nữa

        // ==========================================
        // 3. GOM NHẶT THÔNG TIN NHÉT VÀO HỘP
        // ==========================================

        // --- A. Tọa độ người chơi ---
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            dataToSave.playerPosX = player.transform.position.x;
            dataToSave.playerPosY = player.transform.position.y;
            dataToSave.playerPosZ = player.transform.position.z;
        }

        // --- B. Thời gian (Ngày & Giờ) ---
        TimeManager timeManager = FindDisabledOrActiveObjectOfType<TimeManager>();
        if (timeManager != null)
        {
            dataToSave.currentDay = timeManager.currentDay;   // Hút Ngày
            dataToSave.currentHour = timeManager.currentHour; // Hút Giờ
            dataToSave.currentSacrificedValue = timeManager.CurrentSacrificedValue;
        }
        else
        {
            dataToSave.currentDay = 1;
            dataToSave.currentHour = 6f; // Mặc định 6h sáng nếu lỗi
        }

        // --- C. Kinh tế & Cống phẩm ---
        PlayerWallet wallet = FindDisabledOrActiveObjectOfType<PlayerWallet>();
        if (wallet != null)
        {
            dataToSave.money = wallet.currentGold;
            dataToSave.monsterTributeProgress = 0f; // Hiện tại chỉ dùng wallet để lưu tiền
        }
        else
        {
            GameManager gameManager = FindObjectOfType<GameManager>(); 
            if (gameManager != null)
            {
                dataToSave.money = gameManager.money; // Lấy tiền thực tế
                dataToSave.monsterTributeProgress = gameManager.tributeProgress; // Lấy tiến độ cống phẩm
            }
            else
            {
                dataToSave.money = 0;
                dataToSave.monsterTributeProgress = 0f;
            }
        }

        ShopUIController shopController = FindDisabledOrActiveObjectOfType<ShopUIController>();
        if (shopController != null)
        {
            dataToSave.currentShopLevel = shopController.currentShopLevel;
            dataToSave.currentBagLevel = shopController.currentBagLevel;
        }

        HotbarManager hotbarManager = FindDisabledOrActiveObjectOfType<HotbarManager>();
        if (hotbarManager != null)
        {
            dataToSave.unlockedFishSlots = hotbarManager.unlockedFishSlots;

            // --- D. Kho đồ (Cá, Mồi, Cần câu) ---
            dataToSave.fishInventory = hotbarManager.GetInventoryNames(ItemType.Fish);
            dataToSave.baitInventory = hotbarManager.GetInventoryNames(ItemType.Bait);
            dataToSave.rodInventory = hotbarManager.GetInventoryNames(ItemType.Rod);
        }

        // ==========================================
        // 4. GỌI NGƯỜI VẬN CHUYỂN MANG HỘP ĐI CẤT
        // ==========================================
        SaveSystem.SaveGame(dataToSave);

        // 5. Thoát ra màn hình Main Menu (Scene số 0)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); 
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