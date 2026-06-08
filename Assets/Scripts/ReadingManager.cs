using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReadingManager : MonoBehaviour
{
    public static ReadingManager Instance;

    [Header("Giao diện UI")]
    public GameObject readingPanel; // Kéo vật thể ReadingPanel vào đây
    public Image documentBackgroundImage; // Kéo vật thể ReadingPanel vào đây luôn (để lấy component Image)
    public TextMeshProUGUI documentText; // Kéo vật thể DocumentText vào đây

    [Header("Khóa Di chuyển")]
    [Tooltip("Kéo thả các script di chuyển và xoay camera của Player vào đây để khóa lại khi đang đọc")]
    public MonoBehaviour[] scriptsToDisable; 

    [HideInInspector] public bool isReading = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Thêm một biến đếm ngược thời gian
    private float closeCooldown = 0f;

    private void Update()
    {
        // Trừ lùi thời gian đếm ngược
        if (closeCooldown > 0)
        {
            closeCooldown -= Time.deltaTime;
        }

        // Chỉ cho phép bấm E hoặc Esc để tắt sách khi thời gian chờ đã hết
        if (isReading && closeCooldown <= 0f && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            StopReading();
        }
    }

    public void StartReading(ReadableItem item)
    {
        isReading = true;
        closeCooldown = 0.5f; // Bắt buộc chờ nửa giây mới được phép bấm tắt sách

        // 1. Cập nhật hình ảnh và chữ
        documentBackgroundImage.sprite = item.documentImage;
        documentText.text = item.documentContent;

        // 2. Bật UI lên
        readingPanel.SetActive(true);

        // 3. Khóa di chuyển & xoay camera
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }
    }

    public void StopReading()
    {
        isReading = false;

        // 1. Tắt UI
        readingPanel.SetActive(false);

        // 2. Mở khóa cho Player di chuyển lại bình thường
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = true;
        }
    }
}