using UnityEngine;

public class BaitCollision : MonoBehaviour
{
    private FishingController fishingCtrl;
    private bool hasLanded = false; // Chống chạm 2 lần

    // 1. SỬA LỖI: Đổi Start thành Awake để nó chạy NGAY LẬP TỨC khi mồi vừa sinh ra
    void Awake()
    {
        fishingCtrl = FindObjectOfType<FishingController>();

        // Tự động ép Layer thành "Bait" khi mồi vừa sinh ra
        // Dòng này giúp bạn quên đi nỗi lo phải vào Inspector gán thủ công
        this.gameObject.layer = LayerMask.NameToLayer("Bait");
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasLanded) return;

        // NẾU CHẠM VÀO NƯỚC
        if (other.CompareTag("Water"))
        {
            hasLanded = true;
            WaterZone zone = other.GetComponent<WaterZone>();

            if (zone != null && fishingCtrl != null)
            {
                // Truyền danh sách cá của vùng này về cho Cần câu
                fishingCtrl.BaitLandedInWater(zone.zoneFishDatabase);

                // Giả lập lực đẩy Ác-si-mét
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.useGravity = false;
                }
                Debug.Log("Mồi đã chạm nước khu vực: " + zone.zoneName);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        // 2. SỬA LỖI: Bỏ qua nếu mồi vô tình chạm vào chính người chơi lúc vừa quăng
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("MainCamera"))
        {
            return; // Ngó lơ, không tính là va chạm đất
        }

        // Nếu chạm đất
        if (!collision.collider.isTrigger)
        {
            hasLanded = true;
            Debug.Log("Ném nhầm lên: " + collision.gameObject.name);

            if (fishingCtrl != null)
            {
                fishingCtrl.BaitLandedOnGround();
            }
        }
    }
}