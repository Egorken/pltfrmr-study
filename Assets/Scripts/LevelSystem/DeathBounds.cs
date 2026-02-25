using Game.Combat;
using UnityEngine;

namespace Game.LevelSystem
{
    public class DeathBounds : MonoBehaviour
    {
        [Header("Границы")]
        [SerializeField] private float minY = -20f;
        [SerializeField] private float maxY = 10000f;
        [SerializeField] private float minX = -10000f;
        [SerializeField] private float maxX = 10000f;
        [SerializeField] private string targetTag = "Player";

        private Transform _target;
        private IDamageable _targetDamageable;
        private const int LethalDamage = 9999;

        private void Start()
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null)
            {
                _target = go.transform;
                _targetDamageable = go.GetComponent<IDamageable>();
            }
        }

        private void FixedUpdate()
        {
            if (_target == null)
            {
                TryFindTarget();
                return;
            }

            Vector2 p = _target.position;
            if (p.y < minY || p.y > maxY || p.x < minX || p.x > maxX)
            {
                if (_targetDamageable != null)
                    _targetDamageable.TakeDamage(LethalDamage);
            }
        }

        private void TryFindTarget()
        {
            if (string.IsNullOrEmpty(targetTag)) return;
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null)
            {
                _target = go.transform;
                _targetDamageable = go.GetComponent<IDamageable>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
