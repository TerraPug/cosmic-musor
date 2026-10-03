using UnityEngine;
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class GarbageItem : MonoBehaviour
{
    private Rigidbody body;
    private Collider itemCollider;
    private Transform holdingHand;
    public bool IsHeld { get; private set; }
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
        body.useGravity = false;
        body.linearDamping = 0.2f;
        body.angularDamping = 0.2f;
    }
    public void Grab(Transform hand)
    {
        if (IsHeld || hand == null) return;
        IsHeld = true;
        body.isKinematic = true;
        itemCollider.enabled = false;
        // Предмет не наследует масштаб клешни, в том числе при её поворотах.
        transform.SetParent(null, true);
        holdingHand = hand;
        transform.SetPositionAndRotation(holdingHand.position, holdingHand.rotation);
    }

    private void LateUpdate()
    {
        if (!IsHeld) return;
        if (holdingHand == null)
        {
            Release(Vector3.zero);
            return;
        }

        transform.SetPositionAndRotation(holdingHand.position, holdingHand.rotation);
    }

    private void OnDisable()
    {
        if (IsHeld) Release(Vector3.zero);
    }

    public void Release(Vector3 inheritedVelocity)
    {
        if (!IsHeld) return;
        holdingHand = null;
        itemCollider.enabled = true;
        body.isKinematic = false;
        body.linearVelocity = inheritedVelocity;
        IsHeld = false;
    }
}
