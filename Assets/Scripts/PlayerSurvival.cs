using UnityEngine;
using System.Collections; // Nhớ có dòng này để dùng Coroutine

public class PlayerSurvival : MonoBehaviour
{
    private bool isDrowning = false;
    private bool isGameJustStarted = true; // Cờ bảo vệ lúc mới vào game

    private void Start()
    {
        // Cho người chơi 2 giây bất tử lúc mới load game
        StartCoroutine(SpawnProtection());
    }

    private IEnumerator SpawnProtection()
    {
        yield return new WaitForSeconds(2f);
        isGameJustStarted = false; // Hết 2 giây thì tắt cờ bảo vệ
    }

    private void OnTriggerEnter(Collider other)
    {
        // Nếu đang trong 2 giây bảo vệ đầu game thì bỏ qua mọi va chạm
        if (isGameJustStarted) return; 

        if (other.CompareTag("WaterDeath") && !isDrowning)
        {
            isDrowning = true;
            if (PlayerDeathManager.Instance != null)
            {
                PlayerDeathManager.Instance.StartDrowning(this.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterDeath") && isDrowning)
        {
            isDrowning = false;
            if (PlayerDeathManager.Instance != null)
            {
                PlayerDeathManager.Instance.CancelDrowning();
            }
        }
    }
}