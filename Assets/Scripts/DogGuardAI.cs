using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class DogGuardAI : MonoBehaviour
{
    [Header("Cài đặt Lãnh thổ")]
    public float patrolRadius = 5f; // Bán kính đi loanh quanh chỗ gác
    public float chaseDistance = 8f; // Bán kính phát hiện trộm
    public float giveUpDistance = 15f; // Đuổi ra xa chừng này mét thì quay về
    public float attackDistance = 2f; // Khoảng cách cắn (chạm vào người)

    [Header("Cài đặt Tốc độ")]
    public float walkSpeed = 2f;
    public float runSpeed = 5.5f;

    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private Vector3 guardPos; // Điểm gác gốc của con chó

    private enum DogState { Guarding, Chasing, Returning, Attacking }
    private DogState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>(); 
        
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Lưu lại vị trí ban đầu làm chốt gác
        guardPos = transform.position;
        currentState = DogState.Guarding;

        StartCoroutine(DogLogicRoutine());
    }

    private IEnumerator DogLogicRoutine()
    {
        while (true)
        {
            if (player == null) yield return null;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            switch (currentState)
            {
                // ==========================================
                // 1. ĐỨNG GÁC CỔNG & ĐI LƯỢN LỜ
                // ==========================================
                case DogState.Guarding:
                    agent.speed = walkSpeed;
                    
                    // Thấy người chơi lọt vào tầm nhìn -> Chuyển sang rượt
                    if (distanceToPlayer <= chaseDistance)
                    {
                        currentState = DogState.Chasing;
                        break;
                    }

                    // Đi tuần tra ngẫu nhiên
                    if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    {
                        SetAnimState(false, false); // Đứng im
                        yield return new WaitForSeconds(Random.Range(2f, 4f));
                        
                        Vector3 randomPoint = guardPos + Random.insideUnitSphere * patrolRadius;
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, 1))
                        {
                            agent.SetDestination(hit.position);
                            SetAnimState(true, false); // Bật đi bộ
                        }
                    }
                    break;

                // ==========================================
                // 2. RƯỢT ĐUỔI NGƯỜI CHƠI
                // ==========================================
                case DogState.Chasing:
                    agent.speed = runSpeed;
                    agent.SetDestination(player.position);
                    
                    SetAnimState(false, true); // Bật chạy nhanh

                    // Đuổi kịp tới sát người -> Cắn
                    if (distanceToPlayer <= attackDistance)
                    {
                        currentState = DogState.Attacking;
                        break;
                    }

                    // Người chơi chạy ra khỏi lãnh thổ -> Bỏ cuộc
                    float distanceFromGuardPoint = Vector3.Distance(guardPos, player.position);
                    if (distanceFromGuardPoint > giveUpDistance)
                    {
                        currentState = DogState.Returning;
                    }
                    break;

                // ==========================================
                // 3. QUAY VỀ CHỐT GÁC
                // ==========================================
                case DogState.Returning:
                    agent.speed = walkSpeed;
                    agent.SetDestination(guardPos); 
                    
                    SetAnimState(true, false); // Đi bộ lững thững về nhà

                    // Về đến nhà rồi
                    if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    {
                        currentState = DogState.Guarding;
                    }
                    
                    // Đang đi về mà người chơi lại trêu ngươi -> Rượt tiếp
                    if (distanceToPlayer <= chaseDistance)
                    {
                        currentState = DogState.Chasing;
                    }
                    break;

                // ==========================================
                // 4. TẤN CÔNG (Giết người chơi)
                // ==========================================
                case DogState.Attacking:
                    agent.isStopped = true; // Dừng chạy
                    // Ép con chó luôn quay mặt nhìn chằm chằm vào người chơi
                    transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
                    
                    // Cho chó đứng im (Idle) hoặc bạn có thể đổi thành (false, true) để nó giữ tư thế vồ
                    SetAnimState(false, false); 

                    // Gọi Trọng tài tử vong giáng màn hình nâu đỏ xuống
                    if (PlayerDeathManager.Instance != null)
                    {
                        PlayerDeathManager.Instance.DieByDog(player.gameObject);
                    }

                    // Chờ 3.5 giây cho màn hình UI xử lý xong xuôi
                    yield return new WaitForSeconds(3.5f); 
                    
                    // Người chơi bị biến về nhà rồi, chó được thả ra đi bộ về chốt
                    agent.isStopped = false;
                    currentState = DogState.Returning;
                    break;
            }
            
            yield return null; 
        }
    }

    // Hàm điều khiển Animator giống hệt con gà
    private void SetAnimState(bool isWalking, bool isRunning)
    {
        if (anim != null)
        {
            anim.SetBool("isWalking", isWalking);
            anim.SetBool("isRunning", isRunning);
        }
    }

    // Vẽ vòng màu để dễ căn chỉnh lãnh thổ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? guardPos : transform.position, patrolRadius); 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? guardPos : transform.position, chaseDistance); 
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? guardPos : transform.position, giveUpDistance); 
    }
}