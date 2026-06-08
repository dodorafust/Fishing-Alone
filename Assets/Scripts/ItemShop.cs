using UnityEngine;

public class ItemShop : MonoBehaviour
{
    public string shopName = "Cửa hàng Tạp hóa";
    public ShopUIController shopUI; // Kéo bảng ShopPanel vào đây

    public void OpenTheShop()
    {
        if (shopUI != null) shopUI.OpenShop();
    }
}