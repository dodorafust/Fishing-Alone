using UnityEngine;

public enum FishRarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "New Fish", menuName = "Fishing/Fish Data")]
public class FishData : ItemData
{
    [Header("Thông số Sinh thái")]
    public FishRarity rarity;
    
    [Tooltip("Size mặc định của loài cá này")]
    public float baseSize = 1f;

    [Header("Thông số Minigame (Độ khó cơ bản)")]
    [Tooltip("Kích thước vùng an toàn (Mặc định khoảng 100-150)")]
    public float safeZoneSize = 100f;
    
    [Tooltip("Tốc độ thanh lực giật lên xuống")]
    public float noiseSpeed = 1f;
}