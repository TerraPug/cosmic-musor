using UnityEngine;

public class TwoHandGarbageCollector : MonoBehaviour
{   
    public Transform leftkleshnya;
    public Transform rightkleshnya;
    public float pickupRange = 4f;
    private FreeTrash leftItem;
    private FreeTrash rightItem;
    public bool LeftHandFree => leftItem == null;
    public bool RightHandFree => rightItem == null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    // Вызвать метод хендлхенд два раза  с параметрами нажэатой кнопки мыши 0 или 1 , позиции левой или правой клешни
    void Update()
    {
       HandleHand(0, leftkleshnya, ref leftItem);
       HandleHand(1, rightkleshnya, ref rightItem); 
    }
    void HandleHand(int mouseButton, Transform Hand, ref FreeTrash heldTrash)
    {
        if (Input.GetMouseButtonDown(mouseButton))
        {
            FreeTrash candidate = FindNearestFreeTrash();
            if (candidate != null)
            {
                heldTrash = candidate;
                heldTrash.Grab(Hand);
            }        
        }
    }
    private FreeTrash FindNearestFreeTrash()
    {
      Collider[] hits = Physics.OverlapSphere (transform.position, pickupRange, ~0, QueryTriggerInteraction.Ignore); 
      FreeTrash nearest = null;
      float NearestDistance = float.MaxValue;
      foreach (Collider hit in hits)
        {
            FreeTrash item = hit.GetComponentInParent<FreeTrash>();
            if (item == null) continue;
            float distance = (item.transform.position - transform.position).sqrMagnitude;
            if (distance < NearestDistance)
            {
                NearestDistance = distance;
                nearest = item;
            }
        }

        return nearest;
    }
}
