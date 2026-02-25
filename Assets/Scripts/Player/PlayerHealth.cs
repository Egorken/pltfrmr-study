using System;
using Game.Combat;
using Game.LevelSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 5;

        [Header("Смерть")]
        [SerializeField] private bool respawnAtCheckpoint = true;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDead => CurrentHealth <= 0;

        public event Action<int, int> OnHealthChanged;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Revive()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            if (respawnAtCheckpoint && LevelManager.Instance != null && LevelManager.Instance.HasCheckpoint())
            {
                LevelManager.Instance.RespawnPlayer();
                Revive();
                return;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}

