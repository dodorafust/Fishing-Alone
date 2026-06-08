using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Đã có sẵn IPointerClickHandler để bắt sự kiện Click chuột
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Dữ liệu vật phẩm")]
    public ItemData itemData;

    [HideInInspector] public Transform parentAfterDrag;
    private CanvasGroup canvasGroup;
    private Image image;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        image = GetComponent<Image>();
    }

    private void Start()
    {
        if (itemData != null && itemData.icon != null)
        {
            image.sprite = itemData.icon;
            image.color = Color.white;
        }
        else
        {
            image.color = new Color(1, 1, 1, 0); 
        }

        // BỌC THÉP: Ép mỏ neo tự động bung rộng và vừa khít 100% vào Slot chứa nó
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero; // Neo góc dưới trái
        rectTransform.anchorMax = Vector2.one;  // Neo góc trên phải
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    // ==========================================
    // TÍNH NĂNG 1: CLICK CHUỘT TRÁI ĐỂ GẮN / GỠ MỒI
    // ==========================================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (itemData != null && itemData.itemType == ItemType.Bait)
            {
                FishingController fishing = FindObjectOfType<FishingController>();
                // Bổ sung: Đang câu thì cấm thay mồi
                if (fishing != null && !fishing.isFishing) 
                {
                    // [ĐÃ SỬA]: Chuyển quyền xử lý cho HotbarManager để nó điều khiển Khung viền vàng
                    HotbarManager hotbar = FindObjectOfType<HotbarManager>();
                    if (hotbar != null) 
                    {
                        hotbar.ToggleBait(this); 
                    }
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData) 
    { 
        parentAfterDrag = transform.parent;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null) transform.SetParent(canvas.transform);
        else transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) 
    { 
        transform.position = Input.mousePosition; 
    }

    public void OnEndDrag(PointerEventData eventData) 
    { 
        // ==========================================
        // TÍNH NĂNG 2: VỨT ĐỒ KHI THẢ RA MÀN HÌNH TRỐNG
        // ==========================================
        // Nếu không thả vào ô Slot nào cả (pointerEnter == null) thì sẽ tính là Vứt đi
        if (eventData.pointerEnter == null && itemData != null)
        {
            HotbarManager hotbar = FindObjectOfType<HotbarManager>();
            if (hotbar != null) 
            {
                hotbar.DropItemFromDrag(this);
            }
        }

        transform.SetParent(parentAfterDrag);
        canvasGroup.blocksRaycasts = true; 
        
        // BỌC THÉP: Căn chỉnh lại độ rộng hoàn hảo khi người chơi thả thẻ vào ô mới
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero; 
        rectTransform.anchorMax = Vector2.one;  
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}