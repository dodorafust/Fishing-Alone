using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Shop Data", menuName = "Fishing/Shop Data")]
public class ShopData : ScriptableObject
{
    public List<ItemData> baitList;    // Danh sách mồi có thể mua
    public List<RodData> rodList;      // Danh sách cần câu có thể mua
    
    // Giá nâng cấp túi (theo cấp độ)
    public List<int> bagUpgradePrices = new List<int> { 500, 1000, 2000 };
    // Giá nâng cấp shop
    public List<int> shopUpgradePrices = new List<int> { 1000, 2000, 3000, 5000 };
}