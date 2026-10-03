using UnityEngine;

// Зона станции принимает только свободный мусор.
public class TrashController : MonoBehaviour
{
    public float distance = 5.0f;
    public int DeliveredCount { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled) return;
        GarbageItem item = other.GetComponentInParent<GarbageItem>();
        if (item == null || !item.isActiveAndEnabled || item.IsHeld) return;

        // Исключаем повторную сдачу до отложенного удаления объекта.
        item.gameObject.SetActive(false);
        Destroy(item.gameObject);
        DeliveredCount++;
        Debug.Log($"Сдано мусора: {DeliveredCount}", this);
    }
}
