using System;
using Game.Combat;
using UnityEngine;

namespace Game.Enemy
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;

        public int CurrentHealth { get; private set; }

        public bool IsDead => CurrentHealth <= 0;

        public event Action OnDeath;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            CurrentHealth -= amount;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            OnDeath?.Invoke();
            // TODO: анимация смерти, лут, звук и т.п.
            Destroy(gameObject);
        }
    }
}

