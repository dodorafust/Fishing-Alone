using UnityEngine;

public class FishingMinigame : MonoBehaviour
{
    [Header("Giao diện UI")]
    public GameObject minigamePanel;
    public RectTransform tensionBackground;
    public RectTransform safeZone;
    public RectTransform tensionIndicator;

    [Header("Độ Khó Minigame (Sẽ bị ghi đè bởi Data của Cá)")]
    public float tensionIncreaseRate = 1.2f;
    public float tensionDecreaseRate = 1.0f;
    public float fishStruggleFrequency = 1.5f;
    public float fishStruggleForce = 0.2f;

    [Header("Thời gian Bảo hiểm (Giây)")]
    public float gracePeriod = 2f; 

    public float currentTension = 0f;
    private float tensionHeight;
    [HideInInspector] public bool isPlaying = false;

    private FishingController fishingCtrl;
    private float struggleTimer = 0f;
    private float graceTimer = 0f; 

    void Start()
    {
        fishingCtrl = FindObjectOfType<FishingController>();
        tensionHeight = tensionBackground.rect.height;
        minigamePanel.SetActive(false);
    }

    // ==========================================
    // KHỞI TẠO MINIGAME DỰA TRÊN THÔNG SỐ CÁ VÀ CẦN
    // ==========================================
    public void StartMinigame(FishData fish, RodData rod)
    {
        isPlaying = true;
        
        // 1. Tính toán chỉ số Ổn định từ Cần Câu
        float rodStability = rod != null ? rod.stabilityStat : 0f;
        
        // 2. Thiết lập Vùng An Toàn (Safe Zone)
        // Vùng an toàn gốc của cá + (Mỗi điểm ổn định của cần nới rộng thêm 15 pixel)
        float finalSafeZone = fish.safeZoneSize + (rodStability * 15f);
        safeZone.sizeDelta = new Vector2(safeZone.sizeDelta.x, finalSafeZone);

        // 3. Khởi tạo thuật toán "Độ khó Động"
        SetDifficultyByRarity(fish.rarity, fish.noiseSpeed, rodStability);

        currentTension = 0.5f; // Bắt đầu ở giữa thanh
        graceTimer = gracePeriod; 
        struggleTimer = fishStruggleFrequency;
        
        minigamePanel.SetActive(true);
    }

    private void SetDifficultyByRarity(FishRarity rarity, float baseNoise, float rodStability)
    {
        // Tốc độ chạy mặc định của thanh lực (Cá xịn thì tốc độ chạy tự nhiên cũng nhanh hơn)
        tensionIncreaseRate = 1.0f + (baseNoise * 0.1f);
        tensionDecreaseRate = 1.0f + (baseNoise * 0.05f);

        // 4. Áp dụng Rarity vào nhịp độ giật cục
        switch (rarity)
        {
            case FishRarity.Common:
                fishStruggleForce = 0.05f;     // Giật cực nhẹ (5% thanh)
                fishStruggleFrequency = 2.5f;  // 2.5 giây giật 1 lần
                break;
            case FishRarity.Uncommon:
                fishStruggleForce = 0.1f;      
                fishStruggleFrequency = 2.0f;  
                break;
            case FishRarity.Rare:
                fishStruggleForce = 0.2f;      // Giật trung bình (20% thanh)
                fishStruggleFrequency = 1.2f;  // 1.2 giây giật 1 lần
                break;
            case FishRarity.Epic:
                fishStruggleForce = 0.35f;     // Giật rất mạnh (35% thanh)
                fishStruggleFrequency = 0.8f;  
                break;
            case FishRarity.Legendary:
                fishStruggleForce = 0.5f;      // Giật điên cuồng (Bay nửa thanh)
                fishStruggleFrequency = 0.4f;  // Giật liên tọi (0.4s/lần)
                break;
        }

        // Cần câu xịn giúp kìm hãm lại lực giật của con cá (Giảm tối đa xuống còn 2%)
        float stabilityDampener = rodStability * 0.04f;
        fishStruggleForce = Mathf.Max(0.02f, fishStruggleForce - stabilityDampener);
    }

    // ==========================================
    // VÒNG LẶP UPDATE (XỬ LÝ LỰC GIẬT)
    // ==========================================
    void Update()
    {
        if (!isPlaying) return;

        // Đếm ngược thời gian bảo hiểm
        if (graceTimer > 0) graceTimer -= Time.deltaTime;

        if (Input.GetMouseButton(0)) { currentTension += tensionIncreaseRate * Time.deltaTime; }
        else { currentTension -= tensionDecreaseRate * Time.deltaTime; }

        // Logic Cá Vùng Vẫy (Giật cục ngẫu nhiên)
        struggleTimer -= Time.deltaTime;
        if (struggleTimer <= 0)
        {
            // Radom lực giật lên hoặc xuống
            float randomStruggle = Random.Range(-fishStruggleForce, fishStruggleForce);
            
            // MẸO GAME DESIGN: Cá hiếm có xu hướng giật MẠNH LÊN TRÊN (gây đứt dây) nhiều hơn là thả lỏng.
            if (randomStruggle > 0) randomStruggle *= 1.3f; 
            
            currentTension += randomStruggle;
            
            // Random lại thời gian chờ cho cú giật tiếp theo để người chơi không thể học thuộc nhịp
            struggleTimer = Random.Range(fishStruggleFrequency * 0.5f, fishStruggleFrequency * 1.5f);
        }

        // Giới hạn thanh lực từ 0 đến 1
        currentTension = Mathf.Clamp(currentTension, 0f, 1f);
        tensionIndicator.anchoredPosition = new Vector2(0, currentTension * tensionHeight);

        CheckSafeZone();
    }

    void CheckSafeZone()
    {
        // Công thức tính toán mép dưới và mép trên của Vùng Xanh
        float safeBottom = safeZone.anchoredPosition.y - (safeZone.rect.height * safeZone.pivot.y);
        float safeTop = safeBottom + safeZone.rect.height;
        float currentPos = tensionIndicator.anchoredPosition.y;

        // 1. Kiểm tra xem người chơi có đang giữ kim trong Vùng Xanh không
        bool isSafe = (currentPos >= safeBottom && currentPos <= safeTop);
        bool isProtected = (graceTimer > 0);

        // 2. GỌI LỆNH KÉO DÂY (Đây là mảnh ghép bị thiếu khiến dây không di chuyển)
        // Nếu isSafe = true, dây sẽ tự thu vào. Nếu isSafe = false, cá sẽ bơi ra xa.
        if (fishingCtrl != null)
        {
            fishingCtrl.PullFish(isSafe, isProtected);
        }

        // 3. Xử lý đứt dây/sổng cá ĐỘT NGỘT 
        // Chỉ thua ngay lập tức nếu người chơi để kim chạm đáy (0%) hoặc chạm đỉnh (100%)
        if (!isProtected)
        {
            if (currentTension >= 1f || currentTension <= 0f)
            {
                fishingCtrl.BaitLandedOnGround(); 
                // Ghi chú: Có thể thay bằng fishingCtrl.LoseFish(); nếu bạn có hàm đó
            }
        }
    }

    public void StopMinigame()
    {
        isPlaying = false;
        minigamePanel.SetActive(false);
    }
}