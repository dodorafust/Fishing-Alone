using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalWander : MonoBehaviour
{
    [Header("Cài đặt Đi dạo")]
    public float walkSpeed = 1.5f;
    public float wanderRadius = 8f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 6f;

    [Header("Cài đặt Hoảng sợ")]
    public float runSpeed = 4.5f; // Chạy nhanh hơn đi bộ
    public float fleeDistance = 4f; // Khoảng cách (mét) người chơi đến gần thì bắt đầu chạy
    public float safeDistance = 10f; // Chạy xa khỏi người chơi chừng này mét thì mới thấy an toàn

    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    
    private bool isFleeing = false; // Cờ đánh dấu đang hoảng sợ
    private Coroutine wanderCoroutine; // Biến lưu luồng đi dạo để có thể ngắt ngang

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        
        // Tự động tìm nhân vật của bạn (Đảm bảo nhân vật chính có Tag là "Player")
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Bắt đầu nhịp sống bình thường
        wanderCoroutine = StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        // Nếu không tìm thấy người chơi thì thôi
        if (player == null) return;

        // Tính khoảng cách từ con vật đến người chơi
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Nếu người chơi tiến lại quá gần VÀ con vật chưa chạy -> KÍCH HOẠT BỎ CHẠY!
        if (distanceToPlayer <= fleeDistance && !isFleeing)
        {
            isFleeing = true;
            
            // Dừng ngay việc đi dạo lại
            if (wanderCoroutine != null) StopCoroutine(wanderCoroutine);
            
            // Chuyển sang chế độ chạy thục mạng
            StartCoroutine(FleeRoutine());
        }
    }

    // ==========================================
    // TRẠNG THÁI 1: ĐI DẠO BÌNH THƯỜNG
    // ==========================================
    private IEnumerator WanderRoutine()
    {
        agent.speed = walkSpeed; // Trả lại tốc độ đi bộ

        while (!isFleeing) // Cứ đi dạo mãi nếu không bị hoảng sợ
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
            {
                agent.SetDestination(hit.position);
                if (anim != null) anim.SetBool("isWalking", true); 

                float timeout = 0f;
                while ((agent.pathPending || agent.remainingDistance > 0.2f) && timeout < 10f)
                {
                    timeout += Time.deltaTime; // Cộng dồn thời gian
                    yield return null;
                }

                // Tới nơi thì đứng im mổ thóc
                if (anim != null) anim.SetBool("isWalking", false);
                yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
            }
            else
            {
                yield return null;
            }
        }
    }

    // ==========================================
    // TRẠNG THÁI 2: CHẠY THỤC MẠNG NÉ NGƯỜI
    // ==========================================
    private IEnumerator FleeRoutine()
    {
        agent.speed = runSpeed; // Tăng tốc độ lên mức Chạy
        
        if (anim != null)
        {
            anim.SetBool("isWalking", false); // Tắt đi bộ
            anim.SetBool("isRunning", true);  // Bật hoạt ảnh chạy
        }

        // Chạy liên tục cho đến khi cách xa an toàn
        while (isFleeing)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Nếu đã chạy xa khỏi người chơi đủ an toàn -> Dừng lại
            if (distanceToPlayer > safeDistance)
            {
                isFleeing = false; 
                break;
            }

            // Toán học: Lấy tọa độ con gà TRỪ đi tọa độ người chơi = Hướng đẩy ra xa
            Vector3 directionAwayFromPlayer = transform.position - player.position;
            
            // Chỉ định một điểm đến ở cách con gà 5 mét theo hướng đó
            Vector3 fleePosition = transform.position + directionAwayFromPlayer.normalized * 5f;

            NavMeshHit hit;
            // Tìm vị trí hợp lệ trên mặt đất xanh (NavMesh)
            if (NavMesh.SamplePosition(fleePosition, out hit, 5f, 1))
            {
                agent.SetDestination(hit.position);
            }

            // Cập nhật lại đường chạy mỗi nửa giây (để nếu người chơi đuổi theo thì nó biết bẻ lái)
            yield return new WaitForSeconds(0.5f); 
        }

        // Đã an toàn -> Dừng thở dốc và quay lại đi dạo
        if (anim != null) anim.SetBool("isRunning", false);
        wanderCoroutine = StartCoroutine(WanderRoutine());
    }
}