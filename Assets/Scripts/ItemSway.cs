using UnityEngine;

public class ItemSway : MonoBehaviour
{
    [Header("Sway Vị Trí (Độ trễ khi xoay chuột)")]
    public float positionSwayAmount = 0.02f;
    public float maxPositionSway = 0.06f;
    public float positionSmooth = 6f;

    [Header("Sway Góc Xoay (Nghiêng đồ vật)")]
    public float rotationSwayAmount = 2f;
    public float maxRotationSway = 5f;
    public float rotationSmooth = 8f;

    [Header("Bobbing (Nhấp nhô khi bước đi)")]
    public float bobbingSpeed = 14f;
    public float bobbingAmount = 0.05f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float timer = 0f;

    void Start()
    {
        // Lưu lại vị trí và góc xoay gốc của bàn tay
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        // --- 1. TÍNH TOÁN ĐỘ TRỄ VỊ TRÍ (SWAY) ---
        float mouseX = -Input.GetAxis("Mouse X") * positionSwayAmount;
        float mouseY = -Input.GetAxis("Mouse Y") * positionSwayAmount;
        mouseX = Mathf.Clamp(mouseX, -maxPositionSway, maxPositionSway);
        mouseY = Mathf.Clamp(mouseY, -maxPositionSway, maxPositionSway);

        Vector3 targetPosition = new Vector3(mouseX, mouseY, 0);

        // --- 2. TÍNH TOÁN BƯỚC CHÂN (BOBBING) ---
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // Nếu người chơi đang di chuyển (Bấm WASD)
        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f)
        {
            // Dùng đồ thị hình Sin để tạo nhịp nhấp nhô Lên - Xuống
            timer += Time.deltaTime * bobbingSpeed;
            targetPosition.y += Mathf.Sin(timer) * bobbingAmount;
            
            // Dùng đồ thị hình Cos để tạo nhịp lắc Trái - Phải hình số 8
            targetPosition.x += Mathf.Cos(timer / 2) * (bobbingAmount / 2f);
        }
        else
        {
            // Đứng im thì từ từ trở về số 0
            timer = 0f;
            targetPosition.y = Mathf.Lerp(targetPosition.y, 0, Time.deltaTime * positionSmooth);
        }

        // ÁP DỤNG VỊ TRÍ MỚI (Làm mượt bằng Lerp)
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + targetPosition, Time.deltaTime * positionSmooth);

        // --- 3. TÍNH TOÁN GÓC NGHIÊNG (ROTATION SWAY) ---
        float tiltY = Mathf.Clamp(Input.GetAxis("Mouse X") * rotationSwayAmount, -maxRotationSway, maxRotationSway);
        float tiltX = Mathf.Clamp(Input.GetAxis("Mouse Y") * rotationSwayAmount, -maxRotationSway, maxRotationSway);

        Quaternion targetRotation = Quaternion.Euler(new Vector3(tiltX, -tiltY, 0));
        
        // ÁP DỤNG GÓC NGHIÊNG MỚI
        transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation * targetRotation, Time.deltaTime * rotationSmooth);
    }
}