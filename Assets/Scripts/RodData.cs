using UnityEngine;

[CreateAssetMenu(fileName = "New Rod", menuName = "Fishing/Rod Data")]
public class RodData : ItemData
{
    [Header("Chỉ số RPG Cần Câu")]
    [Tooltip("Cấp độ của cần (1-5)")]
    [Range(1, 5)] public int rodLevel = 1;
    
    [Tooltip("Giảm thời gian cá cắn câu")]
    public float lureStat = 0f; 
    
    [Tooltip("Tăng tỷ lệ ra cá hiếm và cá to")]
    public float luckStat = 0f; 
    
    [Tooltip("Nới rộng vùng an toàn & giảm độ giật")]
    public float stabilityStat = 0f; 

    [Tooltip("Đánh dấu xem đây là cần tre (không thể nâng cấp) hay cần mua")]
    public bool isBambooRod = false;
}