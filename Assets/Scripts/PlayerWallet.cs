using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    public int currentGold = 100; // Tiền khởi đầu
    public TextMeshProUGUI goldText;

    void Start() { UpdateGoldUI(); }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldUI();
            return true;
        }
        return false; // Không đủ tiền
    }

    public void SetGold(int amount)
    {
        currentGold = amount;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText != null) goldText.text = "" + currentGold.ToString() + " G";
    }
}