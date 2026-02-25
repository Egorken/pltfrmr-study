using System.Collections.Generic;
using Game.Combat;
using Game.Weather;
using UnityEngine;

namespace Game.Player
{
    public class PlayerMelee : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRadius = 0.6f;
        [SerializeField] private int damage = 1;

        [Header("Timing")]
        [SerializeField] [Min(0.1f)] private float baseCooldown = 1f;
        [SerializeField] private float attackDuration = 0.2f;

        [Header("Weather")]
        [SerializeField] private WeatherManager weatherManager;

        [Header("Filter")]
        [SerializeField] private LayerMask hitLayers;

        private float _lastAttackTime = -999f;
        private float _attackEndTime;
        private bool _isAttacking;
        private readonly HashSet<Collider2D> _hitThisSwing = new HashSet<Collider2D>();

        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            if (weatherManager == null)
                weatherManager = FindObjectOfType<WeatherManager>();
        }

        private float GetCurrentCooldown()
        {
            float mult = 1f;
            if (weatherManager != null)
                mult = weatherManager.CurrentModifiers.meleeCooldownMultiplier;
            return baseCooldown * Mathf.Max(0.1f, mult);
        }

        private void Update()
        {
            if (_isAttacking)
            {
                if (Time.time >= _attackEndTime)
                {
                    _isAttacking = false;
                    _hitThisSwing.Clear();
                }
                else
                {
                    TryHit();
                }
                return;
            }

            float cooldown = GetCurrentCooldown();
            if (Time.time < _lastAttackTime + cooldown)
                return;

            if (Input.GetButtonDown("Fire1"))
            {
                StartAttack();
            }
        }

        private void StartAttack()
        {
            _lastAttackTime = Time.time;
            _attackEndTime = Time.time + attackDuration;
            _isAttacking = true;
            _hitThisSwing.Clear();
            GetComponent<PlayerAnimator>()?.TriggerAttack();
        }

        private void TryHit()
        {
            Vector2 point = attackPoint != null ? attackPoint.position : transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(point, attackRadius, hitLayers);

            foreach (Collider2D col in hits)
            {
                if (col.gameObject == gameObject) continue;
                if (_hitThisSwing.Contains(col)) continue;

                var damageable = col.GetComponent<IDamageable>();
                if (damageable == null)
                    damageable = col.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    _hitThisSwing.Add(col);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 point = attackPoint != null ? attackPoint.position : transform.position;
            Gizmos.color = _isAttacking ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(point, attackRadius);
        }
    }
}
