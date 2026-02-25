using System.Collections.Generic;
using Game.Inventory;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private TMP_Text listText;

        [Header("Hotkeys")]
        [SerializeField] private bool useNumberKeys = true;

        private void Awake()
        {
            if (inventory == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    inventory = player.GetComponent<PlayerInventory>();
            }

            if (inventory != null)
                inventory.OnInventoryChanged += Refresh;
        }

        private void OnDestroy()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= Refresh;
        }

        private void Update()
        {
            if (inventory == null || !useNumberKeys) return;

            for (int i = 0; i < 9 && i < inventory.Items.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    inventory.UseItem(i);
                    break;
                }
            }
        }

        private void Refresh(List<ItemType> items)
        {
            if (listText == null) return;

            if (items.Count == 0)
            {
                listText.text = "Инвентарь пуст";
                return;
            }

            var lines = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                string name = GetItemName(items[i]);
                if (useNumberKeys && i < 9)
                    lines.Add($"{i + 1}. {name}");
                else
                    lines.Add($"• {name}");
            }

            listText.text = "Инвентарь:\n" + string.Join("\n", lines);
        }

        private static string GetItemName(ItemType item)
        {
            switch (item)
            {
                case ItemType.FullHeal: return "Аптечка (полное)";
                case ItemType.SmallHeal: return "Аптечка (+1)";
                default: return item.ToString();
            }
        }
    }
}
