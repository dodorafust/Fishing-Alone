using UnityEngine;
using System.Collections;

public class FishingController : MonoBehaviour
{
    [Header("Thành phần")]
    public Transform rightHand;
    public Transform mainCamera;
    public Transform inspectPoint;
    public LineRenderer lineRenderer;
    public GameObject baitShellPrefab;

    [Header("Cài đặt Lực quăng")]
    public float throwForce = 15f;
    public float upwardForce = 5f;
    public float maxThrowForce = 35f; // Lực ném tối đa khi gồng đầy thanh
    public float chargeSpeed = 15f;   // Tốc độ tăng lực khi đang giữ chuột
    private float currentThrowForce;  // Lực thực tế sẽ sử dụng khi gọi hàm CastBait

    [Header("Cài đặt Tốc độ Thu dây (Đã điều chỉnh)")]
    public float pullSpeed = 7f;      // Đã tăng tốc độ kéo (Cũ: 2f)
    public float escapeSpeed = 1.5f;
    public float winDistance = 2.5f;  // Nới lỏng khoảng cách thắng (Cũ: 1.5f)
    public float loseDistance = 35f;

    [Header("Cài đặt Thời gian chờ cá")]
    public float baseMinBiteTime = 10f;
    public float baseMaxBiteTime = 15f;

    public Transform currentRodTip;
    private GameObject activeBait;
    private bool isCasting = false;
    private ItemData[] currentZoneFishDatabase;

    [HideInInspector] public bool isFishing = false;

    private bool isWaitingForBite = false;
    private Coroutine fishingTimerCoroutine;

    private ItemData equippedBaitData;
    public RodData equippedRodData;
    private Quaternion defaultHandRotation;

    private FishingMinigame minigame;
    private PlayerMovement playerMovement;

    private FishData currentCaughtFish;
    private float currentCaughtFishSize;

    void Start()
    {
        Application.targetFrameRate = 60; 
        defaultHandRotation = rightHand.localRotation;
        lineRenderer.positionCount = 0;
        
        minigame = GetComponent<FishingMinigame>();
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    // ==========================================
    // CƠ CHẾ DÂY CƯỚC MƯỢT MÀ (LATE UPDATE)
    // ==========================================
    void LateUpdate()
    {
        // ==========================================
        // 1. TỰ ĐỘNG TÌM NGỌN CẦN CÂU (NẾU BỊ MẤT HOẶC VỪA THAY CẦN MỚI)
        // ==========================================
        if (currentRodTip == null)
        {
            // Tìm script RodBender đang nằm trên tay nhân vật
            RodBender bender = GetComponentInChildren<RodBender>();
            
            // Nếu tìm thấy và nó có chứa xương
            if (bender != null && bender.bones.Length > 0)
            {
                // Lấy đốt xương cuối cùng trong danh sách làm mỏ neo dây câu
                currentRodTip = bender.bones[bender.bones.Length - 1]; 
            }
        }

        // ==========================================
        // 2. VẼ DÂY CÂU TỪ NGỌN CẦN ĐẾN PHAO
        // ==========================================
        if (activeBait != null && currentRodTip != null)
        {
            DrawFlexibleLine(currentRodTip.position, activeBait.transform.position);
        }
        else if (lineRenderer.positionCount > 0)
        {
            lineRenderer.positionCount = 0;
        }
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (isFishing && Input.GetKeyDown(KeyCode.X))
            {
                RetractLine();
            }

            if (Input.GetMouseButtonDown(0) && isFishing)
            {
                if (isWaitingForBite && (minigame == null || !minigame.isPlaying))
                {
                    Debug.Log("Kéo cần quá sớm! Cá phát hiện và bơi đi mất.");
                    RetractLine();
                }
            }
            // LẤY ĐÀ VÀ QUĂNG CẦN (Nhấn xuống bắt đầu gồng)
            else if (Input.GetMouseButtonDown(0) && !isCasting && !isFishing && equippedBaitData != null && (minigame == null || !minigame.isPlaying))
            {
                if (playerMovement != null) playerMovement.canMove = false; // KHÓA DI CHUYỂN
                
                isCasting = true; // Kích hoạt trạng thái đang vung cần
                currentThrowForce = throwForce; // Bắt đầu từ lực ném cơ bản
                
                // (GHI CHÚ: Bỏ hàm StartCoroutine(WindUpAnimation()); cũ đi vì ta sẽ tự code animation ngay bên dưới)
            }

            // ====================================================================
            // ĐANG GIỮ CHUỘT (Tăng lực và kéo cần ngả dần ra sau)
            // ====================================================================
            if (Input.GetMouseButton(0) && isCasting && !isFishing)
            {
                // 1. Tăng lực theo thời gian
                currentThrowForce += chargeSpeed * Time.deltaTime;
                currentThrowForce = Mathf.Clamp(currentThrowForce, throwForce, maxThrowForce);

                // 2. Tính toán % gồng lực (từ 0.0 đến 1.0)
                float chargePercent = (currentThrowForce - throwForce) / (maxThrowForce - throwForce);

                // 3. HIỆU ỨNG THỊ GIÁC: Xoay tay cầm từ từ ra sau dựa trên % lực (Tối đa ngả -45 độ)
                float currentAngle = Mathf.Lerp(0f, -45f, chargePercent);
                rightHand.localRotation = defaultHandRotation * Quaternion.Euler(currentAngle, 0f, 0f);
            }

            // THẢ CHUỘT ĐỂ NÉM CẦN
            if (Input.GetMouseButtonUp(0) && isCasting)
            {
                StartCoroutine(ThrowAnimation());
                CastBait(); 
            }
        }
    }

    public void PullFish(bool isSafe, bool isProtected)
    {
        if (activeBait == null) return;

        Vector3 targetPos = new Vector3(rightHand.position.x, activeBait.transform.position.y, rightHand.position.z);
        Vector3 directionToPlayer = (targetPos - activeBait.transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(targetPos, activeBait.transform.position);

        if (isSafe) activeBait.transform.position += directionToPlayer * pullSpeed * Time.deltaTime;
        else activeBait.transform.position -= directionToPlayer * escapeSpeed * Time.deltaTime;

        if (isProtected) return;

        if (distanceToPlayer <= winDistance) CatchFish();
        else if (distanceToPlayer >= loseDistance) LoseFish();
    }

    // Giả sử 'caughtFish' là con cá bạn vừa câu được
    private void OnFishCaught(GameObject fishModel)
    {
        // 1. Cho cá bay về phía người chơi
        StartCoroutine(FishFlyToPlayer(fishModel));
    }

    IEnumerator FishFlyToPlayer(GameObject fishModel)
    {
        Vector3 startPos = fishModel.transform.position;
        Vector3 targetPos = mainCamera.position; // Bay về hướng camera (người chơi)
        float duration = 0.5f; // Bay mất nửa giây
        float time = 0;

        while (time < duration)
        {
            fishModel.transform.position = Vector3.Lerp(startPos, targetPos, time / duration);
            fishModel.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / duration); // Nhỏ dần
            time += Time.deltaTime;
            yield return null;
        }

        // Sau khi bay tới nơi thì xóa model và đưa cá vào túi
        Destroy(fishModel);
        // [Ở đây bạn gọi hàm đưa cá vào kho đồ]
    }

    void LoseFish()
    {
        Debug.Log("Dây cước bị đứt! Cá bơi mất hút.");
        RetractLine();
    }

    void CatchFish()
    {
        // 1. Tắt giao diện Minigame
        FishingMinigame minigame = FindObjectOfType<FishingMinigame>();
        if (minigame != null) minigame.StopMinigame();

        if (currentCaughtFish != null)
        {
            // ====================================================
            // 2. TẠO BẢN SAO ĐỘC LẬP CHO CON CÁ VỪA CÂU ĐƯỢC
            // ====================================================
            FishData uniqueFish = Instantiate(currentCaughtFish);
            uniqueFish.name = currentCaughtFish.name; // Xóa chữ (Clone) ở tên
            
            // Random hệ số kích thước (Từ 0.5x đến 1.5x)
            float randomModifier = Random.Range(0.5f, 1.5f);
            
            // Ép hệ số mới vào BaseSize và Giá tiền
            uniqueFish.baseSize = currentCaughtFish.baseSize * randomModifier;
            uniqueFish.price = Mathf.RoundToInt(currentCaughtFish.price * randomModifier);

            // 3. Kích hoạt hiệu ứng CÁ BAY VÀO NGƯỜI với data cá nhân hóa
            if (uniqueFish.modelPrefab != null)
            {
                Vector3 spawnPos = activeBait != null ? activeBait.transform.position : currentRodTip.position;
                GameObject fishModel = Instantiate(uniqueFish.modelPrefab, spawnPos, Quaternion.identity);
                
                // Lưu ý: truyền uniqueFish và uniqueFish.baseSize vào để lúc bay nó to/nhỏ chuẩn xác
                StartCoroutine(FishFlyToPlayer(fishModel, uniqueFish, uniqueFish.baseSize));
            }
            else
            {
                HotbarManager hotbar = FindObjectOfType<HotbarManager>();
                if (hotbar != null) hotbar.AddFishToInventory(uniqueFish);
            }

            // 4. XỬ LÝ MẤT MỒI
            if (equippedBaitData != null)
            {
                HotbarManager hotbar = FindObjectOfType<HotbarManager>();
                if (hotbar != null) hotbar.ConsumeCurrentBait(equippedBaitData);
                equippedBaitData = null; 
            }
        }
        
        // Dọn dẹp
        RetractLine();
        if (activeBait != null) Destroy(activeBait);
        isFishing = false;
    }   

    public void RetractLine()
    {
        if (fishingTimerCoroutine != null) StopCoroutine(fishingTimerCoroutine);
        if (minigame != null) minigame.StopMinigame();

        isWaitingForBite = false;
        isFishing = false;
        
        if (playerMovement != null) playerMovement.canMove = true; // MỞ KHÓA DI CHUYỂN

        if (activeBait != null) Destroy(activeBait);
        lineRenderer.positionCount = 0;
        
        Debug.Log("Thu cần!");

        // Sinh lại cục mồi lơ lửng nếu vẫn đang cầm cần trên tay
        if (equippedRodData != null) StartCoroutine(SpawnDanglingRoutine());
    }

    public void EquipBait(ItemData baitData) 
    { 
        equippedBaitData = baitData; 
        if (equippedRodData != null && !isFishing && !isCasting) StartCoroutine(SpawnDanglingRoutine());
    }

    public void EquipRod(ItemData rodData) 
    { 
        equippedRodData = rodData as RodData; 
        if (equippedRodData != null) 
        {
            StartCoroutine(SpawnDanglingRoutine());
        }
        else 
        {
            if (activeBait != null) Destroy(activeBait);
            lineRenderer.positionCount = 0;
        }
    }

    // ==========================================
    // VẬT LÝ DÂY TREO - CẢM GIÁC CHÂN THỰC
    // ==========================================
    IEnumerator SpawnDanglingRoutine()
    {
        yield return new WaitForEndOfFrame(); 

        if (activeBait != null) Destroy(activeBait);

        Transform[] allChildren = rightHand.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren) { if (child.name == "RodTip") { currentRodTip = child; break; } }
        if (currentRodTip == null) yield break;

        Rigidbody tipRb = currentRodTip.GetComponent<Rigidbody>();
        if (tipRb == null) { tipRb = currentRodTip.gameObject.AddComponent<Rigidbody>(); tipRb.isKinematic = true; }

        // Mồi cách đầu cần 0.8m hướng xuống
        activeBait = Instantiate(baitShellPrefab, currentRodTip.position - Vector3.up * 0.8f, Quaternion.identity);

        if (equippedBaitData != null && equippedBaitData.modelPrefab != null) { 
            Instantiate(equippedBaitData.modelPrefab, activeBait.transform.position, Quaternion.identity, activeBait.transform); 
        }

        // Khóa khớp vật lý (ConfigurableJoint)
        ConfigurableJoint joint = activeBait.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = tipRb;
        joint.anchor = new Vector3(0, 0.8f, 0); 
        joint.connectedAnchor = Vector3.zero;

        // Khóa chết di chuyển 3 trục (0% co dãn)
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        // Cho phép đung đưa tự do
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        Rigidbody baitRb = activeBait.GetComponent<Rigidbody>();
        if (baitRb != null) {
            baitRb.mass = 2.5f; 
            baitRb.linearDamping = 0.2f;       
            baitRb.angularDamping = 0.5f; 
            baitRb.interpolation = RigidbodyInterpolation.Interpolate; // Sửa lỗi Jitter
        }
    }

    void CastBait()
    {
        if (activeBait == null) return;

        // Phá hủy cái chốt khóa ConfigurableJoint đi để mồi bay tự do
        ConfigurableJoint joint = activeBait.GetComponent<ConfigurableJoint>();
        if (joint != null) Destroy(joint);

        Rigidbody rb = activeBait.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Trả cục mồi về trạng thái nhẹ để ném xa
            rb.mass = 1f;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.05f;
            
            // Xóa sạch vận tốc thừa trước khi ném
            rb.linearVelocity = Vector3.zero;

            Vector3 forceDirection = mainCamera.forward * currentThrowForce + mainCamera.up * upwardForce;
            rb.AddForce(forceDirection, ForceMode.Impulse);
        }

        isFishing = true;
    }

    // ==========================================
    // CÁC HÀM XỬ LÝ VA CHẠM VÀ CHỜ CÁ
    // ==========================================
    public void BaitLandedInWater(ItemData[] localFish)
    {
        if (!isFishing || activeBait == null) return; 

        // [FIX LỖI TRÔI MỒI Ở PHÁT ĐẦU TIÊN] Ép mồi dừng hẳn ngay khi chạm nước
        Rigidbody rb = activeBait.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearDamping = 100f; 
        }

        currentZoneFishDatabase = localFish; 
        isWaitingForBite = true;
        
        if (fishingTimerCoroutine != null) StopCoroutine(fishingTimerCoroutine);
        fishingTimerCoroutine = StartCoroutine(WaitAndBiteRoutine());
    }

    public void BaitLandedOnGround()
    {
        if (!isFishing) return; 
        Invoke("RetractLine", 0.5f); 
    }

    IEnumerator WaitAndBiteRoutine()
    {
        float minTime = baseMinBiteTime; float maxTime = baseMaxBiteTime;
        // TÍNH TOÁN CHỈ SỐ NHỬ CÁ (LURE)
        // TÍNH TOÁN CHỈ SỐ NHỬ CÁ (LURE / BITE TIME REDUCTION)
        float totalLure = 0f;
        if (equippedRodData != null) totalLure += equippedRodData.lureStat;
        
        // Dùng biến biteTimeReduction từ BaitData của bạn
        if (equippedBaitData != null && equippedBaitData is BaitData bait) totalLure += bait.biteTimeReduction;

        // Tính thời gian chờ: Thời gian gốc trừ đi tổng độ Nhử
        float waitTime = Random.Range(baseMinBiteTime, baseMaxBiteTime) - totalLure;
        
        // Đảm bảo thời gian chờ không bị âm, tối thiểu phải đợi 2 giây
        waitTime = Mathf.Max(waitTime, 2f);

        Debug.Log("... Đang chờ cá. Chỉ số Nhử (-" + totalLure + "s). Dự kiến sau " + waitTime.ToString("F1") + " giây");
        yield return new WaitForSeconds(waitTime);

        isWaitingForBite = false;
        
        if (currentZoneFishDatabase != null && currentZoneFishDatabase.Length > 0)
        {
            int randomIndex = Random.Range(0, currentZoneFishDatabase.Length);
            currentCaughtFish = currentZoneFishDatabase[randomIndex] as FishData;
            currentCaughtFishSize = currentCaughtFish.baseSize * Random.Range(0.8f, 1.5f);
            
            Debug.Log($"!!! CÁ CẮN CÂU !!! [{currentCaughtFish.itemName} - Size: {currentCaughtFishSize:F1}]");

            if (minigame != null && !minigame.isPlaying)
            {
                loseDistance = Vector3.Distance(rightHand.position, activeBait.transform.position) + 5f;
                minigame.StartMinigame(currentCaughtFish, equippedRodData);
                
                if (activeBait != null)
                {
                    activeBait.transform.position += Vector3.up * 0.5f; 
                    activeBait.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }
        else
        {
            Debug.LogWarning("Không có cá ở vùng này!");
            RetractLine();
        }
    }

    void DrawFlexibleLine(Vector3 start, Vector3 end)
    {
        int segments = 10; 
        lineRenderer.positionCount = segments;

        Vector3 midPoint = (start + end) / 2f;
        
        // Độ chùng của dây (Ném ra thì căng, cầm tay thì chùng)
        float sagAmount = isFishing ? 0.05f : 0.2f; 
        midPoint.y -= sagAmount;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 point = CalculateBezierPoint(t, start, midPoint, end);
            lineRenderer.SetPosition(i, point);
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    // ==========================================
    // THUẬT TOÁN RANDOM CÁ DỰA TRÊN LUCK & KHOẢNG CÁCH
    // ==========================================
    private FishData ChooseFishWithLuck()
    {
        // 1. Tính tổng độ May Mắn (Luck từ Cần + Attraction Power từ Mồi)
        float totalLuck = 0f;
        if (equippedRodData != null) totalLuck += equippedRodData.luckStat;
        if (equippedBaitData != null && equippedBaitData is BaitData bait) totalLuck += bait.attractionPower;

        // 2. [MỚI] TÍNH TOÁN LUCK DỰA TRÊN KHOẢNG CÁCH QUĂNG MỒI
        if (activeBait != null)
        {
            // Tính khoảng cách (bằng mét) từ người chơi đến vị trí phao câu
            float distance = Vector3.Distance(transform.position, activeBait.transform.position);
            
            // Cứ mỗi 10 mét xa bờ, người chơi được cộng thêm 1 điểm Luck 
            // (Ví dụ quăng xa 35m -> Tăng thêm 3.5 Luck)
            float distanceLuck = distance / 10f; 
            totalLuck += distanceLuck;
            
            Debug.Log($"Khoảng cách quăng mồi: {distance:F1}m - Luck cộng thêm: {distanceLuck:F1} - Tổng Luck: {totalLuck:F1}");
        }

        float totalWeight = 0f;

        if (currentZoneFishDatabase == null || currentZoneFishDatabase.Length == 0)
        {
            Debug.LogWarning("Chưa có danh sách cá trong hồ!");
            return null;
        }

        // Bước 1: Tính tổng tỉ lệ (Weight) của cả hồ cá
        foreach (ItemData item in currentZoneFishDatabase) 
        {
            if (item is FishData fish)
            {
                float weight = GetBaseWeight(fish.rarity); 
                
                // CÔNG THỨC: Luck càng cao (do cần/mồi xịn HOẶC quăng xa), trọng số cá hiếm càng tăng
                if (fish.rarity == FishRarity.Rare || fish.rarity == FishRarity.Epic || fish.rarity == FishRarity.Legendary)
                {
                    weight += totalLuck * 5f; 
                }
                totalWeight += weight;
            }
        }

        // Bước 2: Quay số (Gacha)
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        // Bước 3: Chốt giải thưởng
        foreach (ItemData item in currentZoneFishDatabase) 
        {
            if (item is FishData fish)
            {
                float weight = GetBaseWeight(fish.rarity);
                if (fish.rarity == FishRarity.Rare || fish.rarity == FishRarity.Epic || fish.rarity == FishRarity.Legendary)
                {
                    weight += totalLuck * 5f;
                }

                cumulativeWeight += weight;
                if (randomValue <= cumulativeWeight) return fish; // Trúng con này!
            }
        }
        return null;
    }

    // Tỉ lệ gốc: Cá cùi rất dễ ra, Cá huyền thoại cực khó
    private float GetBaseWeight(FishRarity rarity)
    {
        switch (rarity)
        {
            case FishRarity.Common: return 100f;   // 100 vé
            case FishRarity.Uncommon: return 50f;  // 50 vé
            case FishRarity.Rare: return 15f;      // 15 vé
            case FishRarity.Epic: return 5f;       // 5 vé
            case FishRarity.Legendary: return 1f;  // 1 vé
            default: return 100f;
        }
    }

    IEnumerator WindUpAnimation() { isCasting = true; float time = 0; Quaternion targetRotation = defaultHandRotation * Quaternion.Euler(-40f, 0f, 0f); while (time < 0.3f) { rightHand.localRotation = Quaternion.Lerp(defaultHandRotation, targetRotation, time / 0.3f); time += Time.deltaTime; yield return null; } }
    IEnumerator ThrowAnimation() { float time = 0; Quaternion startRotation = rightHand.localRotation; Quaternion forwardRotation = defaultHandRotation * Quaternion.Euler(20f, 0f, 0f); while (time < 0.15f) { rightHand.localRotation = Quaternion.Lerp(startRotation, forwardRotation, time / 0.15f); time += Time.deltaTime; yield return null; } time = 0; while (time < 0.5f) { rightHand.localRotation = Quaternion.Lerp(forwardRotation, defaultHandRotation, time / 0.5f); time += Time.deltaTime; yield return null; } isCasting = false; }

    // ==========================================
    // HIỆU ỨNG CÁ BAY VÀO NGƯỜI (MINECRAFT STYLE)
    // ==========================================
    private IEnumerator FishFlyToPlayer(GameObject fishModel, FishData fishData, float fishSize)
    {
        if (fishModel == null) yield break;

        Vector3 startPos = fishModel.transform.position;
        Vector3 targetPos = mainCamera.position; // Bay thẳng vào mặt người chơi
        
        float duration = 0.5f; // Thời gian bay (0.5 giây)
        float time = 0;

        // Tính toán kích thước: Từ size thật thu nhỏ lại 5 lần khi gần chạm mặt
        Vector3 targetScale = Vector3.one * fishSize;
        Vector3 endScale = targetScale * 0.2f; 

        while (time < duration)
        {
            if (fishModel == null) yield break; 

            float t = time / duration;
            
            // Bay tịnh tiến kết hợp hàm Sin tạo đường vòng cung nhô lên
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f; 

            fishModel.transform.position = currentPos;
            
            // Xoay lộn nhào lốc xoáy
            fishModel.transform.Rotate(Vector3.right * 15f);

            // Thu nhỏ dần
            fishModel.transform.localScale = Vector3.Lerp(targetScale, endScale, t);

            time += Time.deltaTime;
            yield return null;
        }

        // Tới nơi -> Biến mất
        Destroy(fishModel);
        
        // Chỉ khi cá chạm vào người thì mới thực sự Add vào kho đồ UI
        HotbarManager hotbar = FindObjectOfType<HotbarManager>();
        if (hotbar != null)
        {
            hotbar.AddFishToInventory(fishData);
        }
    }
}