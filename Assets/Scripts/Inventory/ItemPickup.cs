using UnityEngine;

namespace Game.Inventory
{
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemType itemType = ItemType.FullHeal;

        [SerializeField] private string pickupMessage;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null)
                inventory = other.GetComponentInParent<PlayerInventory>();

            if (inventory != null)
            {
                inventory.AddItem(itemType);
                if (!string.IsNullOrEmpty(pickupMessage))
                    Debug.Log(pickupMessage);
                Destroy(gameObject);
            }
        }
    }
}
