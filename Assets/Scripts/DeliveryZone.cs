using UnityEngine;

public sealed class DeliveryZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CarryableItem item = other.GetComponentInParent<CarryableItem>();
        if (item != null) item.Deliver();
    }
}
