using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; // Cần thêm dòng này để dùng List

public class AnimalSpawner : MonoBehaviour
{
    [Header("Cài đặt mẫu vật")]
    public GameObject animalPrefab;

    [Header("Cài đặt số lượng")]
    public int maxAnimalsInArea = 5; 
    public float spawnRadius = 15f;  

    // Cuốn "Sổ Nam Tào": Chỉ quản lý những con vật do chính máy này đẻ ra
    private List<GameObject> spawnedAnimals = new List<GameObject>();

    private void Start()
    {
        SpawnDailyAnimals();
    }

    public void SpawnDailyAnimals()
    {
        // 1. Dọn dẹp sổ sách (Xóa tên những con đã bị người chơi bắt mất)
        spawnedAnimals.RemoveAll(animal => animal == null);
        
        // 2. Tính số lượng cần đẻ thêm (Chỉ đếm quân số trong sổ)
        int animalsToSpawn = maxAnimalsInArea - spawnedAnimals.Count;

        if (animalsToSpawn <= 0) 
        {
            Debug.Log(gameObject.name + ": Đã đủ quân số, không đẻ thêm!");
            return;
        }

        // 3. Tiến hành đẻ
        for (int i = 0; i < animalsToSpawn; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            NavMeshHit hit;
            
            // Đảm bảo vị trí đẻ nằm trên lưới xanh NavMesh
            if (NavMesh.SamplePosition(randomPos, out hit, spawnRadius, 1))
            {
                GameObject newAnimal = Instantiate(animalPrefab, hit.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                
                // Ghi tên con vật mới đẻ vào sổ
                spawnedAnimals.Add(newAnimal); 
            }
        }
        
        Debug.Log(gameObject.name + ": Đã đẻ thêm " + animalsToSpawn + " con.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}