using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeathManager : MonoBehaviour
{
    public static PlayerDeathManager Instance; 

    [Header("UI Cài đặt")]
    public Image deathBackground;
    public TextMeshProUGUI deathText;

    [Header("Điểm hồi sinh khi đuối nước")]
    public Transform shoreRespawnPoint; 

    [Header("Màu sắc Màn hình Chết")]
    public Color monsterDeathColor = new Color(0.8f, 0f, 0f, 1f); // Mặc định Đỏ máu
    public Color drownDeathColor = new Color(0f, 0.1f, 0.2f, 1f); // Mặc định Xanh biển thẳm

    private Coroutine drowningCoroutine;

    private void Awake() 
    { 
        if (Instance == null) Instance = this; 
    }

    private void Start()
    {
        if (deathBackground != null) deathBackground.color = new Color(0f, 0f, 0f, 0f);
        if (deathText != null) deathText.color = new Color(1f, 1f, 1f, 0f);
    }

    // ==========================================
    // 1. CÁI CHẾT DO QUÁI VẬT
    // ==========================================
    public void DieByMonster()
    {
        StartCoroutine(MonsterDeathRoutine());
    }

    private IEnumerator MonsterDeathRoutine()
    {
        // Gán màu nền theo màu bạn chọn trên Inspector (giữ nguyên độ trong suốt alpha = 0 lúc đầu)
        deathBackground.color = new Color(monsterDeathColor.r, monsterDeathColor.g, monsterDeathColor.b, 0f); 
        deathText.text = "BẠN ĐÃ BỊ VẢ CHẾT!";
        deathText.color = new Color(1f, 1f, 1f, 0f);

        float alpha = 0;
        while(alpha < 1)
        {
            alpha += Time.deltaTime * 2.5f; // Tốc độ hiện màn hình (càng to càng nhanh)
            deathBackground.color = new Color(monsterDeathColor.r, monsterDeathColor.g, monsterDeathColor.b, alpha);
            deathText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ==========================================
    // 2. CÁI CHẾT NGẠT NƯỚC (5 GIÂY VÙNG VẪY)
    // ==========================================
    public void StartDrowning(GameObject player)
    {
        if (drowningCoroutine != null) StopCoroutine(drowningCoroutine);
        drowningCoroutine = StartCoroutine(DrowningRoutine(player));
    }

    public void CancelDrowning()
    {
        if (drowningCoroutine != null) StopCoroutine(drowningCoroutine);
        StartCoroutine(FadeOutUI());
    }

    private IEnumerator DrowningRoutine(GameObject player)
    {
        deathBackground.color = new Color(drownDeathColor.r, drownDeathColor.g, drownDeathColor.b, 0f); 
        deathText.text = "CẢNH BÁO ĐUỐI NƯỚC...";
        deathText.color = new Color(1f, 1f, 1f, 0f);

        float timer = 0;
        float timeToDrown = 5f; 

        while (timer < timeToDrown)
        {
            timer += Time.deltaTime;
            float alpha = timer / timeToDrown;
            deathBackground.color = new Color(drownDeathColor.r, drownDeathColor.g, drownDeathColor.b, alpha);
            deathText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        deathText.text = "Bạn đã chết đuối\nĐược Quỷ sông vớt lên bờ!";

        HotbarManager hotbar = FindObjectOfType<HotbarManager>();
        if (hotbar != null) hotbar.ClearAllInventory(); 

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = shoreRespawnPoint.position;
        if (cc != null) cc.enabled = true;

        yield return new WaitForSeconds(3f); 
        
        StartCoroutine(FadeOutUI());
    }

    private IEnumerator FadeOutUI()
    {
        float alpha = deathBackground.color.a;
        while(alpha > 0)
        {
            alpha -= Time.deltaTime * 1.5f;
            Color bg = deathBackground.color; bg.a = alpha; deathBackground.color = bg;
            Color txt = deathText.color; txt.a = alpha; deathText.color = txt;
            yield return null;
        }
    }

    // ==========================================
    // 3. CÁI CHẾT DO CHÓ CẮN (Mất đồ, màn hình Nâu đỏ)
    // ==========================================
    public void DieByDog(GameObject player)
    {
        StartCoroutine(DogDeathRoutine(player));
    }

    private IEnumerator DogDeathRoutine(GameObject player)
    {
        // Màn hình màu Nâu đỏ sậm
        deathBackground.color = new Color(0.4f, 0.1f, 0f, 0f); 
        deathText.text = "Bị CHÓA CẮN MẤT ĐỒ";
        deathText.color = new Color(1f, 1f, 1f, 0f);

        // Mờ dần hiện lên
        float alpha = 0;
        while(alpha < 1)
        {
            alpha += Time.deltaTime * 2f;
            deathBackground.color = new Color(0.4f, 0.1f, 0f, alpha);
            deathText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        // Trừ phạt: Xóa đồ trong kho
        HotbarManager hotbar = FindObjectOfType<HotbarManager>();
        if (hotbar != null) hotbar.ClearAllInventory(); 

        // Dịch chuyển người chơi về điểm hồi sinh (Dùng chung điểm với chết đuối hoặc điểm mới tùy bạn)
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = shoreRespawnPoint.position;
        if (cc != null) cc.enabled = true;

        // Chờ 3 giây cho người chơi đọc dòng chữ
        yield return new WaitForSeconds(3f); 
        
        // Mở mắt ra (Tắt UI)
        StartCoroutine(FadeOutUI());
    }
}