using UnityEngine;

public class TwoHandGarbageCollector : MonoBehaviour
{
    public Transform leftkleshnya;
    public Transform rightkleshnya;
    public float pickupRange = 4f;

    private GarbageItem leftItem;
    private GarbageItem rightItem;
    public Rigidbody body;

    public bool LeftHandFree => leftItem == null;
    public bool RightHandFree => rightItem == null;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleHand(0, leftkleshnya, ref leftItem);
        HandleHand(1, rightkleshnya, ref rightItem);
    }

    private void OnDisable()
    {
        ReleaseItem(ref leftItem);
        ReleaseItem(ref rightItem);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) return;
        ReleaseItem(ref leftItem);
        ReleaseItem(ref rightItem);
    }

    private void ReleaseItem(ref GarbageItem item)
    {
        if (item != null)
            item.Release(body != null ? body.linearVelocity : Vector3.zero);
        item = null;
    }

    private void HandleHand(int mouseButton, Transform hand, ref GarbageItem heldTrash)
    {
        if (heldTrash != null && !heldTrash.IsHeld) heldTrash = null;

        // Предмет удерживается только пока зажата соответствующая кнопка.
        if (heldTrash != null)
        {
            if (!Input.GetMouseButton(mouseButton)) ReleaseItem(ref heldTrash);
            return;
        }

        if (hand == null || !Input.GetMouseButtonDown(mouseButton)) return;
        GarbageItem candidate = FindNearestFreeTrash();
        if (candidate == null) return;
        candidate.Grab(hand);
        if (candidate.IsHeld) heldTrash = candidate;
    }

    private GarbageItem FindNearestFreeTrash()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, ~0, QueryTriggerInteraction.Ignore);
        GarbageItem nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (Collider hit in hits)
        {
            GarbageItem item = hit.GetComponentInParent<GarbageItem>();
            if (item == null || !item.isActiveAndEnabled || item.IsHeld) continue;
            float distance = (item.transform.position - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = item;
            }
        }

        return nearest;
    }
}
