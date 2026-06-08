using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    public float walkSpeed = 5f;        // Tốc độ đi bộ mặc định
    public float sprintSpeed = 8f;      // Tốc độ chạy nhanh
    public float jumpHeight = 1.5f;     // Độ cao của cú nhảy
    public float gravity = -9.81f;      // Gia tốc trọng trường
    public float mouseSensitivity = 300f;

    [Header("Cài đặt Head Bobbing (Bước chân)")]
    public float walkBobSpeed = 14f;    // Nhịp nhấp nhô khi đi bộ
    public float sprintBobSpeed = 18f;  // Nhịp nhấp nhô khi chạy (nhanh hơn)
    public float bobAmount = 0.05f;     // Độ nhấp nhô của Camera

    // TRẠNG THÁI: Cho phép hệ thống khác (như Câu cá) khóa chân nhân vật
    [HideInInspector] public bool canMove = true;

    private CharacterController controller;
    private Transform cam;

    // Các biến ẩn để tính toán
    private float xRotation = 0f;
    private Vector3 velocity;           // Quản lý gia tốc rơi
    private float defaultCamPosY;       // Lưu vị trí gốc của Camera
    private float timer = 0f;           // Bộ đếm thời gian cho bước chân

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>().transform;

        // Lưu lại độ cao mặc định của mắt nhân vật (ví dụ: Y = 0.6)
        defaultCamPosY = cam.localPosition.y;
    }

    void Update()
    {
        // Cập nhật độ nhạy chuột liên tục từ cài đặt của người chơi
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 450f);
        // Chỉ hoạt động khi con trỏ chuột đã bị khóa
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleLook(); // Luôn cho phép xoay đầu ngắm cảnh, dù có bị khóa chân

            // Chỉ cho phép di chuyển và nhấp nhô nếu không bị khóa
            if (canMove)
            {
                HandleMoveAndJump();
                HandleHeadBob();
            }
            else
            {
                // Nếu bị khóa chân, vẫn phải ép trọng lực xuống để tránh lỗi lơ lửng
                ApplyGravity();

                // Từ từ hạ Camera về vị trí gốc nếu đang bị khóa
                ResetHeadBob();
            }
        }
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMoveAndJump()
    {
        // 1. CHẠM ĐẤT
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Ép một lực nhỏ để nhân vật luôn bám sát mặt đất
        }

        // XÁC ĐỊNH TRẠNG THÁI CHẠY/ĐI BỘ
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // 2. DI CHUYỂN WASD
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 3. NHẢY (Nhấn Space)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Công thức vật lý tính lực nhảy
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        ApplyGravity();
    }

    void ApplyGravity()
    {
        // 4. TRỌNG LỰC
        velocity.y += gravity * Time.deltaTime;          // Rơi nhanh dần đều
        controller.Move(velocity * Time.deltaTime);      // Áp dụng lực rơi vào nhân vật
    }

    void HandleHeadBob()
    {
        // Nếu không chạm đất (đang nhảy/rơi) thì không rung màn hình
        if (!controller.isGrounded) return;

        // Kiểm tra xem người chơi có đang bấm phím di chuyển không
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float currentBobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;

            // Dùng hàm Sin để tạo đường cong nhấp nhô trơn tru
            timer += Time.deltaTime * currentBobSpeed;
            cam.localPosition = new Vector3(cam.localPosition.x, defaultCamPosY + Mathf.Sin(timer) * bobAmount, cam.localPosition.z);
        }
        else
        {
            ResetHeadBob();
        }
    }

    void ResetHeadBob()
    {
        // Nếu đứng im, từ từ hạ Camera về vị trí mặc định mượt mà (dùng Lerp)
        timer = 0;
        cam.localPosition = new Vector3(cam.localPosition.x, Mathf.Lerp(cam.localPosition.y, defaultCamPosY, Time.deltaTime * walkBobSpeed), cam.localPosition.z);
    }
}