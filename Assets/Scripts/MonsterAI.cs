using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // Dùng để load lại màn chơi
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    private Animator anim;
    
    [Header("Cài đặt Tấn công")]
    public float attackRange = 2.5f; // Khoảng cách để nó há mồm cắn
    public float timeToDie = 1.5f; // Thời gian chờ hoạt ảnh cắn xong mới thua game

    private bool isAttacking = false;

    void Start()
    {
        // Tự động tìm người chơi (Đảm bảo Player của bạn có Tag là "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Nếu không có player hoặc đang bận cắn thì không làm gì cả
        if (player == null || isAttacking) return;

        // 1. Lệnh đuổi theo Player
        agent.SetDestination(player.position);

        // 2. Kiểm tra khoảng cách: Nếu đến đủ gần -> CẮN!
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            StartCoroutine(AttackAndKillPlayer());
        }
    }

    IEnumerator AttackAndKillPlayer()
    {
        isAttacking = true;
        agent.isStopped = true; 
        
        // 1. Quái vật bắt đầu vung tay / há mồm
        if (anim != null) anim.SetTrigger("AttackTrigger");

        // 2. CHỜ MỘT CHÚT CHO ANIMATION CHẠY TỚI ĐOẠN "VẢ TRÚNG"
        // Bạn có thể tăng giảm số 0.8f này (ví dụ 0.5f hoặc 1.2f) cho khớp với hoạt ảnh thằn lằn của bạn
        yield return new WaitForSeconds(0.8f);

        // 3. Lúc này mới giáng màn hình chết xuống
        if (PlayerDeathManager.Instance != null)
        {
            PlayerDeathManager.Instance.DieByMonster();
        }

        yield break; 
    }
}