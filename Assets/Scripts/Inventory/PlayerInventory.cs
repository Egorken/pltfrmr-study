using System;
using System.Collections.Generic;
using Game.Player;
using UnityEngine;

namespace Game.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;

        private readonly List<ItemType> _items = new List<ItemType>();

        public IReadOnlyList<ItemType> Items => _items;

        public event Action<List<ItemType>> OnInventoryChanged;

        private void Awake()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();
        }

        public void AddItem(ItemType item)
        {
            if (item == ItemType.None) return;

            _items.Add(item);
            OnInventoryChanged?.Invoke(_items);
        }

        public bool UseItem(int index)
        {
            if (index < 0 || index >= _items.Count) return false;

            ItemType item = _items[index];
            if (!ApplyItemEffect(item))
                return false;

            _items.RemoveAt(index);
            OnInventoryChanged?.Invoke(_items);
            return true;
        }

        public bool UseFirstOfType(ItemType itemType)
        {
            int index = _items.IndexOf(itemType);
            return index >= 0 && UseItem(index);
        }

        public bool TryRemoveItem(ItemType itemType)
        {
            int index = _items.IndexOf(itemType);
            if (index < 0) return false;
            _items.RemoveAt(index);
            OnInventoryChanged?.Invoke(_items);
            return true;
        }

        public bool HasItem(ItemType itemType) => _items.Contains(itemType);

        private bool ApplyItemEffect(ItemType item)
        {
            if (playerHealth == null || playerHealth.IsDead)
                return false;

            switch (item)
            {
                case ItemType.FullHeal:
                    int toHeal = playerHealth.MaxHealth - playerHealth.CurrentHealth;
                    if (toHeal <= 0) return false;
                    playerHealth.Heal(toHeal);
                    return true;

                case ItemType.SmallHeal:
                    if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return false;
                    playerHealth.Heal(1);
                    return true;

                default:
                    return false;
            }
        }
    }
}
