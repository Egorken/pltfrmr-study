using System.Collections.Generic;
using Game.Player;
using UnityEngine;

namespace Game.UI
{
    public class HeartsHealthUI : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private GameObject heartPrefab;
        [SerializeField] private Transform heartsContainer;
        [SerializeField] private List<GameObject> heartIcons = new List<GameObject>();

        private List<GameObject> _hearts = new List<GameObject>();

        private void Start()
        {
            if (playerHealth == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) playerHealth = go.GetComponent<PlayerHealth>();
            }

            if (playerHealth == null)
            {
                Debug.LogWarning("[HeartsHealthUI] PlayerHealth не найден.", this);
                return;
            }

            if (heartIcons != null && heartIcons.Count > 0)
            {
                _hearts = new List<GameObject>(heartIcons);
            }
            else if (heartPrefab != null && heartsContainer != null)
            {
                int max = playerHealth.MaxHealth;
                for (int i = 0; i < max; i++)
                {
                    var heart = Instantiate(heartPrefab, heartsContainer);
                    heart.SetActive(true);
                    _hearts.Add(heart);
                }
            }
            else
            {
                Debug.LogWarning("[HeartsHealthUI] Задай Heart Prefab + Hearts Container либо список Heart Icons.", this);
                return;
            }

            playerHealth.OnHealthChanged += OnHealthChanged;
            OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.OnHealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            for (int i = 0; i < _hearts.Count; i++)
            {
                if (_hearts[i] != null)
                    _hearts[i].SetActive(i < current);
            }
        }
    }
}
