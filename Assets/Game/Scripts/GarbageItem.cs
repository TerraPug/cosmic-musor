using UnityEngine;
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class GarbageItem : MonoBehaviour
{
    private Rigidbody body;
    private Collider itemCollider;
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
        if (IsHeld) return;
        IsHeld = true;
        body.isKinematic = true;
        itemCollider.enabled = false;
        transform.SetParent(hand, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
         public void Release(Vector3 inheritedVelocity)
    {
        if (!IsHeld) return;
        transform.SetParent(null, true);
        itemCollider.enabled = true;
        body.isKinematic = false;
        body.linearVelocity = inheritedVelocity;
        IsHeld = false;
    }
}