using UnityEngine;
using TMPro; 
using System.Collections;
using System.Collections.Generic;

public class HotbarManager : MonoBehaviour
{
    [Header("Cài đặt UI")]
    public RectTransform selectionBorder;
    public InventorySlot[] fishSlots;
    public InventorySlot[] rodSlots;
    public InventorySlot[] baitSlots; // Kho mồi (RightInventoryPanel)

    [Header("Dữ liệu Item (nếu cần)")]
    public ItemData[] itemDatabase; // Nếu bạn muốn gán sẵn scriptable object ItemData vào đây

    [Header("UI Thông tin Vật phẩm (Góc trái)")]
    public GameObject itemInfoPanel;
    public TextMeshProUGUI infoNameText;
    public TextMeshProUGUI infoDescText; 
    public TextMeshProUGUI infoStatsText;
    public TextMeshProUGUI infoPriceText;

    [Header("Cài đặt UI Mồi")]
    public RectTransform baitSelectionBorder; // Bạn tạo 1 cái ảnh viền vàng giống selectionBorder và kéo vào đây nhé
    private DraggableItem currentSelectedBait;

    [Header("UI Tên Vật Phẩm")]
    public TextMeshProUGUI itemNameText; 

    [Header("Hệ thống Mở khóa")]
    [Range(1, 9)]
    public int unlockedFishSlots = 6;

    [Header("Vị trí cầm đồ & Vứt đồ")]
    public Transform rightHand;
    
    [Header("Hiệu ứng Đổi đồ")]
    public float equipAnimationDuration = 0.4f; // Thời gian đổi đồ (0.4 giây)
    public Vector3 hiddenPositionOffset = new Vector3(0, -1f, 0); // Kéo tay chúi xuống 1 mét

    private Coroutine handAnimationRoutine;
    private Vector3 originalHandPosition; // Lưu vị trí gốc của cánh tay
    public float dropForwardForce = 4f; 
    public float dropUpwardForce = 2f;  

    // Các biến trạng thái
    private int currentFishIndex = 0;
    private int currentRodIndex = 0;
    private bool isHoldingRod = false;

    private ItemData currentEquippedData;
    private GameObject current3DModel;
    private FishingController fishingCtrl;
    private Coroutine namePopupCoroutine; 

    void Start()
    {
        fishingCtrl = FindObjectOfType<FishingController>();
        if (itemNameText != null) itemNameText.gameObject.SetActive(false);

        if (baitSelectionBorder != null) baitSelectionBorder.gameObject.SetActive(false);

        if (rightHand != null)
        {
            originalHandPosition = rightHand.localPosition;
        }

        Canvas.ForceUpdateCanvases();
        SelectFishSlot(0);
    }

    void Update()
    {
        HandleInput();
        UpdateEquippedItem();
    }

    void HandleInput()
    {
        if (fishingCtrl != null && fishingCtrl.isFishing) return;
        
        // Đổi ô cá (1-9)
        for (int i = 0; i < unlockedFishSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SelectFishSlot(i);
        }

        // Lăn chuột đổi ô cá
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) 
        {
            int nextIndex = currentFishIndex - 1;
            if (nextIndex < 0) nextIndex = unlockedFishSlots - 1;
            SelectFishSlot(nextIndex);
        }
        else if (scroll < 0f) 
        {
            int nextIndex = currentFishIndex + 1;
            if (nextIndex >= unlockedFishSlots) nextIndex = 0;
            SelectFishSlot(nextIndex);
        }

        // Đổi cần câu
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isHoldingRod) SelectRodSlot(currentRodIndex == 0 ? 1 : 0);
            else SelectRodSlot(0);
        }

        // VỨT ĐỒ (Phím B)
        if (Input.GetKeyDown(KeyCode.B))
        {
            DropCurrentItem();
        }
    }

    void SelectFishSlot(int index) 
    { 
        // 1. CẬP NHẬT UI NGAY LẬP TỨC (Không có delay)
        isHoldingRod = false; 
        currentFishIndex = index; 
        selectionBorder.position = fishSlots[index].transform.position; 
        
        // (Nếu bạn có gọi hàm UpdateItemInfoUI để cập nhật chữ, hãy gọi luôn ở đây)

        // 2. CHẠY HIỆU ỨNG TAY 3D
        PlayEquipAnimation(() => 
        {
            // LƯU Ý: Chỉ để lệnh Sinh ra/Thay đổi mô hình cá 3D ở bên trong này!
            // Ví dụ: InstantiateItemInHand(fishSlots[index]); 
        });
    }

    void SelectRodSlot(int index) 
    { 
        if (index >= rodSlots.Length) return; 

        // 1. CẬP NHẬT UI NGAY LẬP TỨC (Không có delay)
        isHoldingRod = true; 
        currentRodIndex = index; 
        selectionBorder.position = rodSlots[index].transform.position; 

        // 2. CHẠY HIỆU ỨNG TAY 3D
        PlayEquipAnimation(() => 
        {
            // LƯU Ý: Chỉ để lệnh Sinh ra/Thay đổi mô hình cần câu 3D ở bên trong này!
            // Ví dụ: SpawnRodInHand(rodSlots[index]);
        });
    }

    void UpdateEquippedItem()
    {
        InventorySlot activeSlot = isHoldingRod ? rodSlots[currentRodIndex] : fishSlots[currentFishIndex];
        ItemData dataInSlot = null;

        DraggableItem item = activeSlot.GetComponentInChildren<DraggableItem>();
        if (item != null) dataInSlot = item.itemData;

        if (dataInSlot != currentEquippedData)
        {
            if (current3DModel != null) Destroy(current3DModel);
            currentEquippedData = dataInSlot;

            if (currentEquippedData != null && currentEquippedData.modelPrefab != null)
            {
                current3DModel = Instantiate(currentEquippedData.modelPrefab, rightHand, false);

                if (currentEquippedData is FishData fishData)
                    current3DModel.transform.localScale = Vector3.one * fishData.baseSize;

                ShowItemNamePopup(currentEquippedData.itemName);
            }

            if (fishingCtrl != null)
            {
                if (currentEquippedData is RodData) fishingCtrl.EquipRod(currentEquippedData);
                else fishingCtrl.EquipRod(null); 
            }
        }
        UpdateItemInfoUI(currentEquippedData);
    }

    public void ShowItemNamePopup(string name)
    {
        if (itemNameText == null) return;
        if (namePopupCoroutine != null) StopCoroutine(namePopupCoroutine);
        namePopupCoroutine = StartCoroutine(PopupRoutine(name));
    }

    // Thêm hàm này vào HotbarManager để các script khác hỏi thăm
    public ItemData GetCurrentEquippedItem()
    {
        return currentEquippedData;
    }

    private IEnumerator PopupRoutine(string name)
    {
        itemNameText.text = name;
        itemNameText.gameObject.SetActive(true); 
        yield return new WaitForSeconds(3f); 
        itemNameText.gameObject.SetActive(false); 
    }

    // ==========================================
    // MỞ KHÓA THÊM Ô TÚI ĐỒ (Gọi từ Shop)
    // ==========================================
    public void UnlockMoreSlots(int amount)
    {
        unlockedFishSlots += amount;

        // Đảm bảo không vượt quá tổng số ô tối đa đang có trên UI
        if (unlockedFishSlots > fishSlots.Length)
        {
            unlockedFishSlots = fishSlots.Length;
        }

        // Gọi hàm cập nhật hiển thị lên màn hình
        UpdateUnlockedSlotsUI();
        Debug.Log("Đã mở rộng túi đồ lên: " + unlockedFishSlots + " ô!");
    }

    // ==========================================
    // CẬP NHẬT HIỂN THỊ CÁC Ô TRÊN MÀN HÌNH
    // ==========================================
    public void UpdateUnlockedSlotsUI()
    {
        if (fishSlots == null) return;

        for (int i = 0; i < fishSlots.Length; i++)
        {
            if (i < unlockedFishSlots)
            {
                // Nếu nằm trong giới hạn đã mở khóa -> Hiện ô này lên
                fishSlots[i].gameObject.SetActive(true);
            }
            else
            {
                // Nếu chưa mở khóa tới -> Ẩn ô này đi
                fishSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // ==========================================
    // NHẬN ĐỒ VÀO TÚI THÔNG MINH
    // ==========================================
    public bool AddItemToInventory(ItemData newItem)
    {
        InventorySlot[] targetSlots = null;
        int maxSlots = 0;

        // Phân loại túi
        if (newItem is FishData) { targetSlots = fishSlots; maxSlots = unlockedFishSlots; }
        else if (newItem is RodData) { targetSlots = rodSlots; maxSlots = rodSlots.Length; }
        else if (newItem is BaitData) { targetSlots = baitSlots; maxSlots = baitSlots != null ? baitSlots.Length : 0; }

        if (targetSlots == null || targetSlots.Length == 0) return false;

        // [LOGIC MỚI]: Tìm ô TRỐNG (itemData == null) trước
        for (int i = 0; i < maxSlots; i++)
        {
            if (i >= targetSlots.Length) break;
            
            DraggableItem item = targetSlots[i].GetComponentInChildren<DraggableItem>(true);
            
            // Nếu ô đó tồn tại và chưa có dữ liệu (đang trống)
            if (item != null && item.itemData == null) 
            {
                item.itemData = newItem;
                UpdateSlotUI(item, newItem); // Hàm cập nhật ảnh icon
                return true; // Nhận hàng thành công
            }
        }
        
        Debug.LogWarning("Tất cả ô chứa " + newItem.itemType + " đã đầy!");
        return false; // Túi đầy, không thể mua/nhặt thêm
    }

    // Hàm bổ trợ để cập nhật UI gọn gàng
    private void UpdateSlotUI(DraggableItem item, ItemData data)
    {
        UnityEngine.UI.Image img = item.GetComponent<UnityEngine.UI.Image>();
        if (img != null && data.icon != null)
        {
            img.gameObject.SetActive(true);
            img.sprite = data.icon;
            img.color = Color.white;
        }
    }

    // CHỈ GIỮ LẠI ĐÚNG 1 HÀM NÀY ĐỂ TRÁNH LỖI CS0111
    public bool AddFishToInventory(ItemData newFish) 
    { 
        return AddItemToInventory(newFish); 
    }

    // ==========================================
    // Lấy tên vật phẩm trong kho để lưu
    // ==========================================
    public List<string> GetInventoryNames(ItemType itemType)
    {
        InventorySlot[] targetSlots = GetSlotsForType(itemType);
        List<string> result = new List<string>();

        if (targetSlots == null) return result;

        for (int i = 0; i < targetSlots.Length; i++)
        {
            DraggableItem item = targetSlots[i].GetComponentInChildren<DraggableItem>(true);
            if (item != null && item.itemData != null)
            {
                result.Add(item.itemData.itemName);
            }
        }

        return result;
    }

    public void LoadInventoryFromNames(List<string> fishNames, List<string> baitNames, List<string> rodNames)
    {
        ClearAllInventory();

        FillSlotsWithNames(ItemType.Fish, fishNames, unlockedFishSlots);
        FillSlotsWithNames(ItemType.Bait, baitNames, baitSlots != null ? baitSlots.Length : 0);
        FillSlotsWithNames(ItemType.Rod, rodNames, rodSlots != null ? rodSlots.Length : 0);
    }

    private void FillSlotsWithNames(ItemType type, List<string> names, int slotCount)
    {
        if (names == null) return;

        InventorySlot[] targetSlots = GetSlotsForType(type);
        if (targetSlots == null) return;

        for (int i = 0; i < names.Count && i < slotCount && i < targetSlots.Length; i++)
        {
            ItemData itemData = FindItemDataByName(type, names[i]);
            if (itemData != null)
            {
                DraggableItem item = targetSlots[i].GetComponentInChildren<DraggableItem>(true);
                if (item != null)
                {
                    item.itemData = itemData;
                    UpdateSlotUI(item, itemData);
                }
            }
        }
    }

    private InventorySlot[] GetSlotsForType(ItemType type)
    {
        if (type == ItemType.Fish) return fishSlots;
        if (type == ItemType.Bait) return baitSlots;
        if (type == ItemType.Rod) return rodSlots;
        return null;
    }

    private ItemData FindItemDataByName(ItemType type, string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;

        ItemData[] allItems = GetAllItemData();
        if (allItems == null || allItems.Length == 0)
        {
            Debug.LogWarning($"Không có ItemData nào được tìm thấy để load kho.");
            return null;
        }

        foreach (ItemData item in allItems)
        {
            if (item != null && item.itemType == type && item.itemName == itemName)
            {
                return item;
            }
        }

        Debug.LogWarning($"Không tìm thấy ItemData tên '{itemName}' loại {type} để load kho.");
        return null;
    }

    private ItemData[] GetAllItemData()
    {
        if (itemDatabase != null && itemDatabase.Length > 0)
        {
            return itemDatabase;
        }

        ItemData[] found = Resources.FindObjectsOfTypeAll<ItemData>();
        if (found != null && found.Length > 0) return found;

#if UNITY_EDITOR
        List<ItemData> editorItems = new List<ItemData>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null) editorItems.Add(item);
        }
        return editorItems.ToArray();
#else
        return new ItemData[0];
#endif
    }

    // ==========================================
    // VỨT ĐỒ XUỐNG ĐẤT
    // ==========================================
    public void DropCurrentItem()
    {
        if (currentEquippedData == null) return;

        if (currentEquippedData.modelPrefab != null)
        {
            Transform camTransform = Camera.main.transform;
            GameObject droppedObj = Instantiate(currentEquippedData.modelPrefab, camTransform.position + camTransform.forward * 1.5f, Quaternion.identity);

            if (currentEquippedData is FishData fishData)
            {
                droppedObj.transform.localScale = Vector3.one * fishData.baseSize;
            }

            Rigidbody rb = droppedObj.GetComponent<Rigidbody>();
            if (rb == null) rb = droppedObj.AddComponent<Rigidbody>();
            rb.AddForce(camTransform.forward * dropForwardForce + camTransform.up * dropUpwardForce, ForceMode.Impulse);

            PickupItem pickupScript = droppedObj.AddComponent<PickupItem>();
            pickupScript.itemData = currentEquippedData;

            Collider col = droppedObj.GetComponent<Collider>();
            if (col == null) droppedObj.AddComponent<BoxCollider>();
        }

        // Xóa đồ đang cầm trên tay khỏi UI
        InventorySlot activeSlot = isHoldingRod ? rodSlots[currentRodIndex] : fishSlots[currentFishIndex];
        if (activeSlot != null)
        {
            DraggableItem item = activeSlot.GetComponentInChildren<DraggableItem>();
            if (item != null && item.itemData == currentEquippedData)
            {
                item.itemData = null;
                UnityEngine.UI.Image img = item.GetComponent<UnityEngine.UI.Image>();
                if (img != null) { img.sprite = null; img.gameObject.SetActive(false); }
            }
        }

        currentEquippedData = null;
        if (current3DModel != null) Destroy(current3DModel);
        if (itemNameText != null) itemNameText.gameObject.SetActive(false);
    }

    // ==========================================
    // TIÊU HAO MỒI CÂU KHI CÁ CẮN
    // ==========================================
    public void ConsumeCurrentBait(ItemData consumedBait)
    {
        if (consumedBait == null || baitSlots == null) return;

        foreach (InventorySlot slot in baitSlots)
        {
            DraggableItem item = slot.GetComponentInChildren<DraggableItem>(true);
            
            if (item != null && item.itemData == consumedBait)
            {
                item.itemData = null; 
                UnityEngine.UI.Image img = item.GetComponent<UnityEngine.UI.Image>();
                if (img != null) { img.sprite = null; img.gameObject.SetActive(false); }
                break; // Chỉ xóa 1 cục mồi rồi dừng lại
            }
        }
    }

    // ==========================================
    // XÓA SẠCH ĐỒ KHI CHẾT
    // ==========================================
    public void ClearAllInventory()
    {
        if (fishSlots != null) { foreach (InventorySlot slot in fishSlots) { DraggableItem item = slot.GetComponentInChildren<DraggableItem>(true); if (item != null) { item.itemData = null; UnityEngine.UI.Image img = item.GetComponent<UnityEngine.UI.Image>(); if (img != null) { img.sprite = null; img.gameObject.SetActive(false); } } } }
        if (rodSlots != null) { foreach (InventorySlot slot in rodSlots) { DraggableItem item = slot.GetComponentInChildren<DraggableItem>(true); if (item != null) { item.itemData = null; UnityEngine.UI.Image img = item.GetComponent<UnityEngine.UI.Image>(); if (img != null) { img.sprite = null; img.gameObject.SetActive(false); } } } }
        if (baitSlots != null) { foreach (InventorySlot slot in baitSlots) { DraggableItem item = slot.GetComponentInChildren<DraggableItem>(true); if (item != null) { item.itemData = null; UnityEngine.UI.Image img = item.GetComponent<UnityEngine.UI.Image>(); if (img != null) { img.sprite = null; img.gameObject.SetActive(false); } } } }

        if (current3DModel != null) Destroy(current3DModel);
        currentEquippedData = null;
        
        if (fishingCtrl != null) { fishingCtrl.EquipRod(null); fishingCtrl.EquipBait(null); }
        if (itemNameText != null) itemNameText.gameObject.SetActive(false);
    }

    // ==========================================
    // XỬ LÝ CLICK: GẮN HOẶC GỠ MỒI CÂU
    // ==========================================
    public void ToggleBait(DraggableItem clickedBait)
    {
        // Chỉ xử lý nếu đồ được click là mồi câu (BaitData)
        if (clickedBait.itemData == null || !(clickedBait.itemData is BaitData)) return;

        // Nếu bạn muốn: Chỉ cho phép chọn mồi nếu đang cầm cần câu (isHoldingRod)
        if (!isHoldingRod) 
        {
            Debug.LogWarning("Bạn phải cầm cần câu trên tay mới gắn mồi được!");
            return;
        }

        // Nếu bấm lại vào đúng cục mồi đang chọn -> GỠ MỒI
        if (currentSelectedBait == clickedBait)
        {
            currentSelectedBait = null;
            if (baitSelectionBorder != null) baitSelectionBorder.gameObject.SetActive(false); // Giấu viền
            if (fishingCtrl != null) fishingCtrl.EquipBait(null); // Gỡ mồi khỏi cần
            Debug.Log("Đã GỠ mồi.");

            // MỚI THÊM: Vừa gỡ mồi xong thì trả bảng UI về hiển thị thông số Cây cần câu
            UpdateItemInfoUI(currentEquippedData);
        }
        else // Nếu bấm vào cục mồi mới -> GẮN MỒI
        {
            currentSelectedBait = clickedBait;
            if (baitSelectionBorder != null)
            {
                baitSelectionBorder.gameObject.SetActive(true); // Hiện viền
                baitSelectionBorder.position = clickedBait.transform.position; // Nhảy viền tới ô vừa click
            }
            if (fishingCtrl != null) fishingCtrl.EquipBait(clickedBait.itemData); // Gắn lên cần
            Debug.Log("Đã GẮN mồi: " + clickedBait.itemData.itemName);

            // MỚI THÊM: Hiện bảng thông tin của cục Mồi vừa bấm vào
            UpdateItemInfoUI(clickedBait.itemData);
        }
    }

    // Thêm hàm này vào HotbarManager.cs để các script khác có thể đọc được đồ đang cầm
    public ItemData GetCurrentEquippedData()
    {
        return currentEquippedData;
    }

    // ==========================================
    // XỬ LÝ: VỨT ĐỒ KHI KÉO THẢ RA MÀN HÌNH TRỐNG
    // ==========================================
    public void DropItemFromDrag(DraggableItem draggedItem)
    {
        if (draggedItem == null || draggedItem.itemData == null) return;
        ItemData dataToDrop = draggedItem.itemData;

        // 1. Sinh mô hình 3D rớt ra đất
        if (dataToDrop.modelPrefab != null)
        {
            Transform camTransform = Camera.main.transform;
            GameObject droppedObj = Instantiate(dataToDrop.modelPrefab, camTransform.position + camTransform.forward * 1.5f, Quaternion.identity);

            // Fix lỗi cá khổng lồ
            if (dataToDrop is FishData fishData) droppedObj.transform.localScale = Vector3.one * fishData.baseSize;

            // [FIX LỖI CRASH Ở ĐÂY]: Viết rõ ràng, không dùng toán tử viết tắt ?? nữa
            Rigidbody rb = droppedObj.GetComponent<Rigidbody>();
            if (rb == null) rb = droppedObj.AddComponent<Rigidbody>();
            
            rb.isKinematic = false; // Đảm bảo vật thể có thể rớt xuống
            rb.AddForce(camTransform.forward * dropForwardForce + camTransform.up * dropUpwardForce, ForceMode.Impulse);

            PickupItem pickupScript = droppedObj.AddComponent<PickupItem>();
            pickupScript.itemData = dataToDrop;

            Collider col = droppedObj.GetComponent<Collider>();
            if (col == null) droppedObj.AddComponent<BoxCollider>();
        }

        // 2. Xử lý gỡ mồi trên cần câu (Nếu cái bị vứt đang là mồi đang gắn)
        if (currentSelectedBait == draggedItem) 
        {
            currentSelectedBait = null;
            if (baitSelectionBorder != null) baitSelectionBorder.gameObject.SetActive(false);
            if (fishingCtrl != null) fishingCtrl.EquipBait(null); // Ra lệnh tháo mồi khỏi cần câu 3D
        }

        // 3. Xóa data trên ô UI (Để icon biến mất đàng hoàng)
        draggedItem.itemData = null;
        UnityEngine.UI.Image img = draggedItem.GetComponent<UnityEngine.UI.Image>();
        if (img != null) { img.sprite = null; img.gameObject.SetActive(false); }

        UpdateEquippedItem(); // Tự động cập nhật lại đồ cầm trên tay
    }

    // ==========================================
    // HÀM DỌN DẸP ĐỒ ĐANG CẦM TAY (Dùng khi bán đồ)
    // ==========================================
    public void ClearEquippedItem()
    {
        if (currentEquippedData == null) return;

        // 1. Tìm ô đang cầm đồ trên UI
        InventorySlot activeSlot = isHoldingRod ? rodSlots[currentRodIndex] : fishSlots[currentFishIndex];
        if (activeSlot != null)
        {
            DraggableItem item = activeSlot.GetComponentInChildren<DraggableItem>();
            if (item != null && item.itemData == currentEquippedData)
            {
                item.itemData = null; // Xóa dữ liệu
                UnityEngine.UI.Image img = item.GetComponent<UnityEngine.UI.Image>();
                if (img != null) { img.sprite = null; img.gameObject.SetActive(false); }
            }
        }

        // 2. Hủy model 3D trên tay và đặt data về null
        if (current3DModel != null) Destroy(current3DModel);
        currentEquippedData = null;
        
        // 3. Giấu tên vật phẩm
        if (itemNameText != null) itemNameText.gameObject.SetActive(false);
        
        Debug.Log("Đã xóa vật phẩm khỏi tay.");
    }

    // ==========================================
    // CẬP NHẬT BẢNG THÔNG TIN GÓC TRÁI
    // ==========================================
    public void UpdateItemInfoUI(ItemData item)
    {
        if (item == null)
        {
            if (itemInfoPanel != null) itemInfoPanel.SetActive(false);
            return;
        }

        if (itemInfoPanel != null) itemInfoPanel.SetActive(true);

        if (infoNameText != null) infoNameText.text = item.itemName;
        if (infoDescText != null) infoDescText.text = item.description;
        if (infoPriceText != null) infoPriceText.text = "Giá: " + item.price + " Vàng";

        // Phân loại thông số hiển thị dựa trên loại Item
        if (infoStatsText != null)
        {
            if (item is FishData fish)
            {
                // Hiển thị Độ hiếm và Kích thước có 1 chữ số thập phân (F1)
                infoStatsText.text = $"Độ hiếm: {fish.rarity}\nKích thước: {fish.baseSize:F1}";
            }
            else if (item is RodData rod)
            {
                // Hiển thị 3 chỉ số của cần câu
                infoStatsText.text = $"Lực nhử: {rod.lureStat}\nMay mắn: {rod.luckStat}\nỔn định: {rod.stabilityStat}";
            }
            else 
            {
                // Dành cho Mồi câu
                infoStatsText.text = "Phân loại: Mồi câu"; 
            }
        }
    }

    // ==========================================
    // HIỆU ỨNG ĐỔI ĐỒ (PROCEDURAL ANIMATION)
    // ==========================================
    public void PlayEquipAnimation(System.Action changeItemAction)
    {
        // Nếu đang có hiệu ứng chạy dở, dừng nó lại để chạy hiệu ứng mới
        if (handAnimationRoutine != null) StopCoroutine(handAnimationRoutine);
        handAnimationRoutine = StartCoroutine(EquipRoutine(changeItemAction));
    }

    private IEnumerator EquipRoutine(System.Action changeItemAction)
    {
        if (rightHand == null) yield break;

        Vector3 startPos = rightHand.localPosition;
        Vector3 hiddenPos = originalHandPosition + hiddenPositionOffset;

        // 1. Kéo tay chúi xuống màn hình (Cất đồ)
        float timer = 0;
        while (timer < equipAnimationDuration / 2)
        {
            timer += Time.deltaTime;
            float percent = timer / (equipAnimationDuration / 2);
            // Dùng SmoothStep để tay giật xuống tự nhiên hơn
            rightHand.localPosition = Vector3.Lerp(startPos, hiddenPos, Mathf.SmoothStep(0, 1, percent));
            yield return null;
        }

        // 2. THỰC HIỆN LỆNH ĐỔI ĐỒ (Thay mô hình cá/cần câu lúc tay đang khuất)
        if (changeItemAction != null)
        {
            changeItemAction.Invoke(); 
        }

        // 3. Hất tay lên lại (Rút đồ mới)
        timer = 0;
        while (timer < equipAnimationDuration / 2)
        {
            timer += Time.deltaTime;
            float percent = timer / (equipAnimationDuration / 2);
            rightHand.localPosition = Vector3.Lerp(hiddenPos, originalHandPosition, Mathf.SmoothStep(0, 1, percent));
            yield return null;
        }

        // 4. Đảm bảo tay về đúng vị trí chuẩn xác
        rightHand.localPosition = originalHandPosition;
    }
}