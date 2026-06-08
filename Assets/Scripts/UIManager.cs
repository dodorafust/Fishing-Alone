using UnityEngine;
using System.Collections; 

public class UIManager : MonoBehaviour
{
    public GameObject rightInventory; 
    public GameObject selectionFrame; 

    private bool isInventoryOpen = false;
    
    // MỚI THÊM: Biến này đóng vai trò như "bộ nhớ" để giữ trạng thái của khung chọn
    private bool shouldShowFrame = false; 

    void Start()
    {
        // Tắt mọi thứ một cách triệt để khi vừa vào game (Fix lỗi 1)
        isInventoryOpen = false;
        shouldShowFrame = false;
        
        if (selectionFrame != null) 
        {
            selectionFrame.SetActive(false);
        }
        rightInventory.SetActive(false);
        
        CloseInventory();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isInventoryOpen = !isInventoryOpen;

            if (isInventoryOpen) 
            {
                OpenInventory();
            }
            else 
            {
                CloseInventory();
            }
        }
    }

    void OpenInventory()
    {
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;                  
        
        rightInventory.SetActive(true);
        
        StartCoroutine(WaitAndShowSelectionFrame());
    }

    IEnumerator WaitAndShowSelectionFrame()
    {
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame(); 

        // FIX LỖI 2: CHỈ bật khung lên NẾU trước đó nó đang được hiển thị 
        // (tức là người chơi đang có mồi được chọn)
        if (selectionFrame != null && shouldShowFrame) 
        {
            selectionFrame.SetActive(true);
        }
    }

    void CloseInventory()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;                   

        rightInventory.SetActive(false);
        
        if (selectionFrame != null) 
        {
            // MẤU CHỐT NẰM Ở ĐÂY:
            // Lưu lại trạng thái của khung chọn TRƯỚC KHI tắt nó đi
            // - Nếu bạn đã hủy chọn (khung đang tắt), biến này sẽ lưu là 'false'.
            // - Nếu bạn đang chọn mồi (khung đang bật), biến này sẽ lưu là 'true'.
            shouldShowFrame = selectionFrame.activeSelf;
            
            // Sau khi nhớ xong thì mới tắt nó đi để không bị vướng trên màn hình
            selectionFrame.SetActive(false);
        }
                
        HotbarManager hotbar = FindObjectOfType<HotbarManager>();
        if (hotbar != null)
        {
            hotbar.UpdateItemInfoUI(hotbar.GetCurrentEquippedData()); 
        }
    }
}