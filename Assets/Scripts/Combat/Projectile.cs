using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifeTime = 5f;

        [SerializeField] private string targetTag = "Player";

        private Vector2 _direction = Vector2.right;
        private float _timer;
        private Transform _owner;

        public void Initialize(Vector2 direction, Transform owner, int damageOverride = -1, float speedOverride = -1f)
        {
            _direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            _owner = owner;

            if (damageOverride > 0)
                damage = damageOverride;

            if (speedOverride > 0f)
                speed = speedOverride;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);

            _timer += Time.deltaTime;
            if (_timer >= lifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Игнорируем владельца (и его детей), чтобы не ловить собственные пули
            if (_owner != null && (other.transform == _owner || other.transform.IsChildOf(_owner)))
                return;

            // Если задан targetTag, бьём только объекты с этим тегом
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
                return;

            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}

