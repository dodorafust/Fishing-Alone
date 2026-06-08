using UnityEngine;

public class RodBender : MonoBehaviour
{
    [Header("Cài đặt Xương cần câu")]
    [Tooltip("Kéo thả các đốt xương vào đây (Từ gốc đến ngọn)")]
    public Transform[] bones; 
    
    [Tooltip("Góc cong tối đa của mỗi đốt xương (Độ)")]
    public float maxBendAnglePerBone = 15f;
    
    [Tooltip("Trục uốn cong (Tùy theo model của bạn, thường là X hoặc Z)")]
    public Vector3 bendAxis = Vector3.right; // right = trục X, forward = trục Z

    private FishingMinigame minigame;
    private Quaternion[] initialRotations;

    void Start()
    {
        minigame = FindObjectOfType<FishingMinigame>();

        // Lưu lại góc xoay mặc định ban đầu của các đốt xương
        initialRotations = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            initialRotations[i] = bones[i].localRotation;
        }
    }

    void Update()
    {
        float tension = 0f;
        
        // Lấy dữ liệu lực căng từ Minigame
        if (minigame != null && minigame.isPlaying)
        {
            tension = minigame.currentTension; // Chạy từ 0.0 đến 1.0
        }

        // Thực hiện uốn cong các đốt xương
        for (int i = 0; i < bones.Length; i++)
        {
            // Càng gần ngọn (i càng lớn), cần câu càng dễ bị uốn cong hơn
            float bendingMultiplier = (float)(i + 1) / bones.Length;
            
            // Tính toán góc cong
            float targetAngle = tension * maxBendAnglePerBone * bendingMultiplier;

            // HIỆU ỨNG RUNG LẮC: Khi cá giật (tension > 0), cần câu sẽ rung bần bật
            if (tension > 0.1f)
            {
                targetAngle += Mathf.Sin(Time.time * 30f) * tension * 2f; 
            }

            // Áp dụng góc xoay mới vào xương
            bones[i].localRotation = initialRotations[i] * Quaternion.Euler(bendAxis * targetAngle);
        }
    }
}