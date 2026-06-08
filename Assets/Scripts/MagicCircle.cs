using UnityEngine;
using System.Collections;

// 1. "Tem" đánh dấu con cá đã qua lò luyện
public class TransmutedFlag : MonoBehaviour { }

public class MagicCircle : MonoBehaviour
{
    [Header("Cài đặt Lò luyện")]
    public Transform spawnPoint;             // Kéo vị trí bạn muốn cá rơi ra
    public Light magicGlowLight;             // Kéo đèn vào đây
    public float minSize = 0.5f;             // Tỉ lệ nhỏ nhất
    public float maxSize = 2.5f;             // Tỉ lệ lớn nhất

    private void OnTriggerEnter(Collider other)
    {
        GameObject fishObject = GetFishObject(other);
        if (fishObject == null) return;

        if (fishObject.GetComponentInParent<TransmutedFlag>() != null) return;

        PickupItem pickup = fishObject.GetComponentInParent<PickupItem>();
        if (pickup != null && pickup.itemData != null && pickup.itemData.isTransmuted) return;

        fishObject.AddComponent<TransmutedFlag>();
        StartCoroutine(TransmuteFishRoutine(fishObject));
    }

    private GameObject GetFishObject(Collider other)
    {
        if (other.CompareTag("Fish")) return other.gameObject;
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Fish")) return other.attachedRigidbody.gameObject;
        if (other.transform.root != null && other.transform.root.CompareTag("Fish")) return other.transform.root.gameObject;
        return null;
    }

    private IEnumerator TransmuteFishRoutine(GameObject originalFish)
    {
        if (originalFish == null) yield break;

        originalFish.SetActive(false);

        if (magicGlowLight != null) magicGlowLight.enabled = true;
        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : originalFish.transform.position;
        GameObject newFish = Instantiate(originalFish, spawnPosition, Quaternion.identity);
        newFish.name = originalFish.name;
        if (newFish.GetComponent<TransmutedFlag>() == null)
        {
            newFish.AddComponent<TransmutedFlag>();
        }

        float randomScale = Random.Range(minSize, maxSize);
        newFish.transform.localScale = originalFish.transform.localScale * randomScale;

        var originalPickup = originalFish.GetComponent<PickupItem>();
        var newPickup = newFish.GetComponent<PickupItem>();
        if (originalPickup != null && newPickup != null && originalPickup.itemData != null)
        {
            newPickup.itemData = Instantiate(originalPickup.itemData);
            newPickup.itemData.isTransmuted = true;

            if (newPickup.itemData is FishData newFishData)
            {
                newFishData.baseSize *= randomScale;
                newFishData.price = Mathf.RoundToInt(newFishData.price * randomScale);
                newFish.transform.localScale = Vector3.one * newFishData.baseSize;
            }
        }

        newFish.SetActive(true);
        Destroy(originalFish);

        yield return new WaitForSeconds(0.5f);
        if (magicGlowLight != null) magicGlowLight.enabled = false;
    }
}