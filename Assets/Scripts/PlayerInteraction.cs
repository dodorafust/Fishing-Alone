using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Cài đặt Nhặt đồ")]
    public float interactDistance = 3f;
    public TextMeshProUGUI interactPromptText; 

    private HotbarManager hotbar;
    private TimeManager timeManager;

    void Start()
    {
        hotbar = FindObjectOfType<HotbarManager>();
        timeManager = FindObjectOfType<TimeManager>();
        if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        // THÊM DÒNG NÀY: Nếu đang mải đọc thư thì không cho phép tia Raycast quét tìm đồ vật khác nữa
        if (ReadingManager.Instance != null && ReadingManager.Instance.isReading) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // 1. NHẶT ĐỒ
            PickupItem pickup = hit.collider.GetComponent<PickupItem>();
            if (pickup != null)
            {
                ShowPrompt("Nhấn [E] để nhặt " + pickup.itemData.itemName);
                if (Input.GetKeyDown(KeyCode.E)) 
                { 
                    if (hotbar.AddItemToInventory(pickup.itemData)) { HidePrompt(); Destroy(pickup.gameObject); } 
                }
                return;
            }

            // 2. BÁN CÁ
            FishTrader trader = hit.collider.GetComponent<FishTrader>();
            if (trader != null)
            {
                ShowPrompt("Nhấn [E] để Bán Cá");
                if (Input.GetKeyDown(KeyCode.E)) trader.SellEquippedFish();
                return;
            }

            // 3. CỬA HÀNG
            ItemShop shop = hit.collider.GetComponent<ItemShop>();
            if (shop != null)
            {
                ShowPrompt("Nhấn [E] để Mở Cửa Hàng");
                if (Input.GetKeyDown(KeyCode.E)) shop.OpenTheShop();
                return;
            }

            // 4. CÁI GIƯỜNG (ĐÃ SỬA LỖI)
            if (hit.collider.CompareTag("Bed"))
            {
                ShowPrompt("Nhấn [E] để Ngủ qua ngày");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    timeManager.TrySleep();
                }
                return; // Đã thêm return để không bị tắt chữ
            }

            // 5. CÁI GIẾNG HIẾN TẾ (THAY ĐỔI THEO Ý TƯỞNG MỚI)
            else if (hit.collider.CompareTag("Well"))
            {
                ItemData heldItem = hotbar.GetCurrentEquippedData();
                if (heldItem is FishData heldFish)
                {
                    // Hiện chữ báo hiệu ném xuống giếng
                    ShowPrompt($"Nhấn [E] ném {heldFish.itemName} xuống giếng (+{heldFish.price} Năng lượng)");
                    
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        timeManager.AddSacrifice(heldFish.price); // Gửi giá trị cá vào hệ thống
                        hotbar.ClearEquippedItem(); // Xóa con cá trên tay
                    }
                }
                else
                {
                    ShowPrompt("Hãy cầm một con cá trên tay để ném xuống giếng!");
                }
                return;
            }

            // 6. BẮT MỒI SỐNG (GÀ, RẮN, GIUN...) ngoài tự nhiên
            LiveAnimal animal = hit.collider.GetComponent<LiveAnimal>();
            if (animal != null)
            {
                // Nếu là loài nguy hiểm (VD: Rắn)
                if (animal.isDangerous)
                {
                    ShowPrompt("Nguy hiểm! Cần Bao tay (hoặc công cụ) để bắt " + animal.baitData.itemName);
                    // Tương lai ta sẽ thêm điều kiện: Nếu có bao tay trong kho -> Bắt được.
                }
                else // Nếu là loài hiền (VD: Gà, Giun)
                {
                    ShowPrompt("Nhấn [E] để Bắt " + animal.baitData.itemName);
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        // Thêm vào kho đồ, nếu kho còn chỗ thì xóa con vật ngoài map
                        if (hotbar.AddItemToInventory(animal.baitData)) 
                        { 
                            HidePrompt(); 
                            Destroy(animal.gameObject); 
                        }
                    }
                }
                return; // Dừng tại đây để không bị tắt chữ
            }

            // 7. ĐỌC THƯ / SÁCH
            ReadableItem readable = hit.collider.GetComponent<ReadableItem>();
            if (readable != null)
            {
                ShowPrompt("Nhấn [E] đọc " + readable.promptMessage);
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    HidePrompt(); // Tắt chữ báo hiệu đi
                    ReadingManager.Instance.StartReading(readable); // Bật giao diện đọc lên
                }
                return; // Dừng lại ở đây
            }
        }

        HidePrompt(); // Tắt chữ nếu không nhìn vào đâu cả
    }

    void ShowPrompt(string msg)
    {
        if (interactPromptText != null) 
        { 
            interactPromptText.text = msg; 
            interactPromptText.gameObject.SetActive(true); 
        }
    }

    void HidePrompt()
    {
        if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);
    }
}