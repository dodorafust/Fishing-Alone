using UnityEngine;

public class WaterZone : MonoBehaviour
{
    [Header("Loại nước (Ao, Sông, Biển...)")]
    public string zoneName = "Khu vực nước";

    [Header("Danh sách Cá của khu vực này")]
    public ItemData[] zoneFishDatabase;
}