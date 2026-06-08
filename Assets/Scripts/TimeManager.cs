using UnityEngine;
using TMPro;
using UnityEngine.UI; // Dùng để điều khiển thanh Slider (Pin)
using System.Collections; // Bắt buộc phải có dòng này để dùng Coroutine (IEnumerator)

public class TimeManager : MonoBehaviour
{
    [Header("Ánh sáng & Thời gian")]
    public Light directionalLight;
    public float realSecondsPerDay = 120f;
    public float startHour = 6f;
    public float deadlineHour = 20f;

    [Header("Hiệu ứng Ngủ")]
    public Image fadeScreen; // Kéo vật thể FadeScreen vào đây
    public float fadeSpeed = 1.5f; // Tốc độ nhắm/mở mắt

    [Header("UI & Hiệu ứng")]
    public TextMeshProUGUI timeText;
    public GameObject rainEffect;
    public GameObject monster;

    [Header("HỆ THỐNG HIẾN TẾ THEO GIÁ TRỊ")]
    public int currentDay = 1;
    public float currentHour
    {
        get { return currentTime; }
        set { currentTime = value; }
    }
    public int baseTargetValue = 500; // Ngày 1 cần 500
    public int targetIncreasePerDay = 300; // Mỗi ngày tăng thêm 300
    private int dailyTargetValue;
    private int currentSacrificedValue = 0;

    [Header("Giao diện Bia Đá (Cục Pin)")]
    public Slider batterySlider; // Kéo thanh Slider vào đây
    public Image batteryFillImage; // Kéo phần Fill của Slider vào đây

    private float currentTime;
    private bool isDeadlineReached = false;

    void Start()
    {
        StartNewDay();
    }

    void Update()
    {
        // ==========================================
        // KHU VỰC HACK/CHEAT CHỈ DÀNH CHO DEVELOPER
        // ==========================================
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            AddSacrifice(500); 
            Debug.Log("Hack: Đã nạp 500 năng lượng!");
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            currentTime = 19.8f; 
            Debug.Log("Hack: Tua nhanh thời gian đến sát giờ chết!");
        }
#endif
        // ==========================================
        if (isDeadlineReached) return;

        currentTime += (24f / realSecondsPerDay) * Time.deltaTime;
        float lightRotation = (currentTime / 24f) * 360f - 90f;
        if (directionalLight != null)
            directionalLight.transform.rotation = Quaternion.Euler(lightRotation, 170f, 0f);

        UpdateTimeUI();

        if (currentTime >= 18.5f && rainEffect != null && !rainEffect.activeSelf)
            rainEffect.SetActive(true);

        if (currentTime >= deadlineHour)
        {
            isDeadlineReached = true;
            TriggerDeadline();
        }
    }

    void UpdateTimeUI()
    {
        if (timeText != null)
        {
            int hours = Mathf.FloorToInt(currentTime);
            int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
            timeText.text = string.Format("Ngày {0} - {1:00}:{2:00}", currentDay, hours, minutes);
        }
    }

    // ===============================================
    // LOGIC NẠP PIN & ĐỔI MÀU
    // ===============================================
    public void AddSacrifice(int fishPriceValue)
    {
        currentSacrificedValue += fishPriceValue;
        UpdateBatteryUI();
    }

    void UpdateBatteryUI()
    {
        if (batterySlider == null || batteryFillImage == null) return;

        // Tính phần trăm pin (Từ 0.0 đến 1.0)
        float percent = (float)currentSacrificedValue / dailyTargetValue;
        batterySlider.value = Mathf.Clamp01(percent); // Giới hạn không cho vượt quá 100%

        // 5 Mức màu tương ứng với 5 vạch
        if (percent >= 1f) batteryFillImage.color = new Color(0f, 0.5f, 1f); // 5 Vạch: Xanh biển
        else if (percent >= 0.8f) batteryFillImage.color = Color.green;      // 4 Vạch: Xanh lá
        else if (percent >= 0.6f) batteryFillImage.color = Color.yellow;     // 3 Vạch: Vàng
        else if (percent >= 0.4f) batteryFillImage.color = new Color(1f, 0.5f, 0f); // 2 Vạch: Cam
        else if (percent > 0f) batteryFillImage.color = Color.red;           // 1 Vạch: Đỏ
        else batteryFillImage.color = Color.clear;                           // 0 Vạch: Tàng hình
    }

    public int CurrentSacrificedValue
    {
        get { return currentSacrificedValue; }
        set
        {
            currentSacrificedValue = value;
            UpdateBatteryUI();
        }
    }

    public void LoadTimeState(int day, float hour, int sacrificedValue)
    {
        currentDay = day;
        currentTime = hour;
        currentSacrificedValue = sacrificedValue;
        dailyTargetValue = baseTargetValue + (targetIncreasePerDay * (currentDay - 1));
        isDeadlineReached = currentTime >= deadlineHour;
        UpdateBatteryUI();
        UpdateTimeUI();

        if (rainEffect != null)
            rainEffect.SetActive(currentTime >= 18.5f);

        if (monster != null)
            monster.SetActive(isDeadlineReached && currentSacrificedValue < dailyTargetValue);
    }

    public void TrySleep()
    {
        if (currentTime < 18f)
        {
            Debug.Log("Chưa tới tối!"); return;
        }

        if (currentSacrificedValue >= dailyTargetValue)
        {
            // Thay vì chuyển ngày ngay lập tức, ta gọi hiệu ứng nhắm mắt
            StartCoroutine(SleepRoutine());
        }
        else
        {
            Debug.LogWarning("Chưa sạc đủ Pin! Sợ quá không ngủ được!");
        }
    }

    // Luồng xử lý hiệu ứng ngủ
    IEnumerator SleepRoutine()
    {
        // 1. Màn hình tối dần (Nhắm mắt)
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            if (fadeScreen != null) fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null; // Đợi khung hình tiếp theo
        }

        // 2. Màn hình đã đen kịt -> Đổi ngày, reset hệ thống ở đây
        currentDay++;
        StartNewDay();

        // 3. Đợi 1.5 giây trong bóng tối (Cảm giác thời gian trôi)
        yield return new WaitForSeconds(1.5f);

        // 4. Màn hình sáng dần lên (Mở mắt đón bình minh)
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            if (fadeScreen != null) fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    void StartNewDay()
    {
        currentTime = startHour;
        isDeadlineReached = false;
        currentSacrificedValue = 0;

        // Tính mục tiêu cho ngày mới (VD: Ngày 1 = 500, Ngày 2 = 800)
        dailyTargetValue = baseTargetValue + (targetIncreasePerDay * (currentDay - 1));

        UpdateBatteryUI(); // Reset thanh pin về 0

        if (rainEffect != null) rainEffect.SetActive(false);
        if (monster != null) monster.SetActive(false);
        // Lệnh gọi máy đẻ động vật làm việc
        AnimalSpawner[] allSpawners = FindObjectsOfType<AnimalSpawner>();
        
        // Bắt từng chuồng một đẻ con vật của nó
        foreach (AnimalSpawner spawner in allSpawners)
        {
            spawner.SpawnDailyAnimals();
        }
    }

    void TriggerDeadline()
    {
        if (currentSacrificedValue >= dailyTargetValue)
        {
            Debug.Log("Đã sạc đầy Bia đá. An toàn!");
        }
        else
        {
            if (monster != null) monster.SetActive(true);
        }
    }
}