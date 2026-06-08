using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopUIController : MonoBehaviour
{
    [Header("Dữ liệu & Liên kết")]
    public ShopData shopData;
    public GameObject contentGrid;
    public GameObject itemButtonPrefab;
    public PlayerWallet playerWallet; // Ví tiền của bạn
    public HotbarManager hotbar;      // Để add đồ vào kho

    [Header("Right Panel Info")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button buyButton;

    [Header("Hệ thống Nâng cấp")]
    public int currentShopLevel = 1;
    public int currentBagLevel = 0; // Số lần đã nâng cấp túi

    private ItemData selectedItem;
    private string selectedUpgradeType = ""; // Đánh dấu đang chọn nâng Túi hay nâng Shop
    private int currentPrice = 0;

    void Start()
    {
        buyButton.gameObject.SetActive(false);
        ClearGrid();
    }

    // ===================================
    // CÁC HÀM CỦA 3 NÚT BẤM (CỘT TRÁI)
    // ===================================
    // 1. SỬA HÀM SHOW BAIT: Chỉ hiện đồ nếu cấp Shop đủ yêu cầu
    public void ShowBaitTab()
    {
        ClearGrid();
        foreach (var bait in shopData.baitList)
        {
            if (bait.requiredShopLevel <= currentShopLevel) CreateItemButton(bait);
        }
    }

    // 2. SỬA HÀM SHOW ROD: Chỉ hiện đồ nếu cấp Shop đủ yêu cầu
    public void ShowRodTab()
    {
        ClearGrid();
        foreach (var rod in shopData.rodList)
        {
            if (rod.requiredShopLevel <= currentShopLevel) CreateItemButton(rod);
        }
    }

    // ===================================
    // HIỂN THỊ THÔNG TIN & XỬ LÝ (CỘT PHẢI)
    // ===================================
    private void CreateItemButton(ItemData item)
    {
        GameObject btnObj = Instantiate(itemButtonPrefab, contentGrid.transform);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = item.itemName;
        
        // Tìm object con có tên chính xác là "ItemIcon"
        Transform iconTransform = btnObj.transform.Find("ItemIcon"); 
        if (iconTransform != null)
        {
            Image iconImg = iconTransform.GetComponent<Image>();
            
            if (item.icon != null) 
            {
                iconImg.sprite = item.icon;
                iconImg.gameObject.SetActive(true);
            }
            else
            {
                iconImg.gameObject.SetActive(false); 
            }
        }
        else 
        {
            // Báo lỗi ra Console để bạn dễ phát hiện nếu sai tên Prefab
            Debug.LogWarning("Không tìm thấy object nào tên là 'ItemIcon' bên trong itemButtonPrefab!");
        }

        btnObj.GetComponent<Button>().onClick.AddListener(() => SelectItem(item));
    }

    public void ShowUpgradeTab()
    {
        ClearGrid();

        // 1. Tạo nút Nâng cấp Túi
        GameObject bagBtn = Instantiate(itemButtonPrefab, contentGrid.transform);
        bagBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Túi đồ +1";
        bagBtn.GetComponent<Button>().onClick.AddListener(SelectBagUpgrade);
        
        // ĐÃ SỬA: Đồng bộ tên tìm kiếm thành "ItemIcon"
        Transform bagIcon = bagBtn.transform.Find("ItemIcon");
        if (bagIcon != null) bagIcon.gameObject.SetActive(false);

        // 2. Tạo nút Nâng cấp Shop
        GameObject shopBtn = Instantiate(itemButtonPrefab, contentGrid.transform);
        shopBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Nâng cấp Shop";
        shopBtn.GetComponent<Button>().onClick.AddListener(SelectShopUpgrade);
        
        // ĐÃ SỬA: Đồng bộ tên tìm kiếm thành "ItemIcon"
        Transform shopIcon = shopBtn.transform.Find("ItemIcon");
        if (shopIcon != null) shopIcon.gameObject.SetActive(false);
    }

    // 3. SỬA HÀM SELECT ITEM: Lấy mô tả thật từ ItemData hiển thị ra
    private void SelectItem(ItemData item)
    {
        selectedItem = item;
        selectedUpgradeType = ""; 
        currentPrice = item.price;

        nameText.text = item.itemName;
        // Lấy mô tả (description) từ ScriptableObject thay vì dòng chữ ảo
        descText.text = "Giá: " + item.price + " G\n\n" + item.description;
        
        buyButton.gameObject.SetActive(true);
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuySelectedItem);
    }

    private void SelectBagUpgrade()
    {
        selectedItem = null;
        selectedUpgradeType = "BAG";

        if (currentBagLevel >= shopData.bagUpgradePrices.Count)
        {
            nameText.text = "<color=yellow>Túi đồ (TỐI ĐA)</color>";
            descText.text = "Bạn đã có một chiếc túi rộng nhất có thể!";
            buyButton.gameObject.SetActive(false); // Ẩn nút mua
        }
        else
        {
            currentPrice = shopData.bagUpgradePrices[currentBagLevel];
            nameText.text = "Nâng cấp Túi đồ +1";
            descText.text = "Giá: " + currentPrice + " G\n\nMở rộng kho chứa cá của bạn thêm 1 ô.";
            
            buyButton.gameObject.SetActive(true);
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyUpgrade);
        }
    }

    private void SelectShopUpgrade()
    {
        selectedItem = null;
        selectedUpgradeType = "SHOP";

        if (currentShopLevel >= 5)
        {
            nameText.text = "<color=yellow>Cửa Hàng (TỐI ĐA)</color>";
            descText.text = "Cửa hàng đã đạt cấp độ 5. Không thể nâng cấp thêm!";
            buyButton.gameObject.SetActive(false); // Ẩn nút mua
        }
        else
        {
            currentPrice = shopData.shopUpgradePrices[currentShopLevel - 1]; // Lấy giá theo cấp
            nameText.text = "Nâng cấp Shop Cấp " + (currentShopLevel + 1);
            descText.text = "Giá: " + currentPrice + " G\n\nGiúp Cửa hàng mở khóa thêm các loại mồi và cần câu xịn sò hơn.";
            
            buyButton.gameObject.SetActive(true);
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyUpgrade);
        }
    }

    // ===================================
    // THANH TOÁN
    // ===================================
    // 4. SỬA HÀM MUA ĐỒ (SỬA LỖI MẤT TIỀN OAN)
    private void BuySelectedItem()
    {
        // Kiểm tra xem có đủ tiền trước không
        if (playerWallet.currentGold >= currentPrice)
        {
            // THỬ nhét đồ vào túi trước. Hàm AddItemToInventory sẽ trả về TRUE nếu nhét vừa, FALSE nếu túi/ô cần câu đã đầy
            if (hotbar.AddItemToInventory(selectedItem))
            {
                // Nhét vừa rồi thì mới trừ tiền!
                playerWallet.SpendGold(currentPrice);
                Debug.Log("TING TING! Mua thành công: " + selectedItem.itemName);
            }
            else
            {
                Debug.LogWarning("Túi đồ hoặc ô Cần câu đã ĐẦY! Hãy vứt bớt đồ trước khi mua.");
                // Bạn có thể update text trên UI để báo cho người chơi biết túi đầy
            }
        }
        else
        {
            Debug.LogWarning("Bạn không có đủ tiền!");
        }
    }

    private void BuyUpgrade()
    {
        if (playerWallet.SpendGold(currentPrice))
        {
            if (selectedUpgradeType == "BAG")
            {
                hotbar.UnlockMoreSlots(1);
                currentBagLevel++;
                SelectBagUpgrade(); // F5 lại màn hình ngay lập tức
            }
            else if (selectedUpgradeType == "SHOP")
            {
                currentShopLevel++;
                SelectShopUpgrade(); // F5 lại màn hình ngay lập tức
            }
            Debug.Log("TING TING! Nâng cấp thành công!");
        }
    }

    private void ClearGrid()
    {
        foreach (Transform child in contentGrid.transform) Destroy(child.gameObject);
        buyButton.gameObject.SetActive(false);
        nameText.text = "Chọn mục bên trái";
        descText.text = "Hàng tuyển chọn, giá cả phải chăng.";
    }

    // ===================================
    // MỞ / ĐÓNG CỬA HÀNG
    // ===================================
    public void OpenShop() 
    { 
        gameObject.SetActive(true); // Hiện bảng Shop
        Cursor.lockState = CursorLockMode.None; // Mở khóa chuột để click UI
        Cursor.visible = true;
        ShowBaitTab(); // Vừa mở shop lên là load luôn danh sách Mồi
    }

    public void CloseShop() 
    { 
        gameObject.SetActive(false); // Ẩn bảng Shop
        Cursor.lockState = CursorLockMode.Locked; // Khóa chuột lại để đi chuyển
        Cursor.visible = false;
    }
}