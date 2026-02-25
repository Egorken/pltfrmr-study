using Game.Combat;
using Game.Weather;
using UnityEngine;

namespace Game.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyAI : MonoBehaviour
    {
        private enum State
        {
            Patrol,
            Attack
        }

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Transform player;
        [SerializeField] private WeatherManager weatherManager;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float waypointThreshold = 0.1f;
        [SerializeField] private float waitAtPointTime = 1f;

        [Header("Attack")]
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float shootingInterval = 1.5f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int projectileDamage = 1;

        private State _state = State.Patrol;
        private int _currentPatrolIndex;
        private float _waitTimer;
        private float _shootTimer;
        private WeatherModifiers _mods = WeatherModifiers.Identity;

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (weatherManager == null)
            {
                weatherManager = FindObjectOfType<WeatherManager>();
            }

            if (weatherManager != null)
            {
                _mods = weatherManager.CurrentModifiers;
                weatherManager.OnWeatherChanged += HandleWeatherChanged;
            }
        }

        private void OnDestroy()
        {
            if (weatherManager != null)
            {
                weatherManager.OnWeatherChanged -= HandleWeatherChanged;
            }
        }

        private void HandleWeatherChanged(WeatherType type, WeatherModifiers mods)
        {
            _mods = mods;
        }

        private void Update()
        {
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }

            UpdateState();
        }

        private void FixedUpdate()
        {
            switch (_state)
            {
                case State.Patrol:
                    PatrolUpdate();
                    break;
                case State.Attack:
                    AttackUpdate();
                    break;
            }

            // Ветер влияет на врага в любом состоянии
            ApplyWind();
        }

        private void UpdateState()
        {
            if (player == null)
            {
                _state = State.Patrol;
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            _state = distanceToPlayer <= detectionRange ? State.Attack : State.Patrol;
        }

        #region Patrol

        private void PatrolUpdate()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                return;
            }

            Transform targetPoint = patrolPoints[_currentPatrolIndex];
            Vector2 direction = (targetPoint.position - transform.position);
            direction.y = 0f;

            if (direction.magnitude <= waypointThreshold)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);

                _waitTimer += Time.fixedDeltaTime;
                if (_waitTimer >= waitAtPointTime)
                {
                    _waitTimer = 0f;
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
                }
            }
            else
            {
                direction.Normalize();
                float speed = moveSpeed * _mods.moveSpeedMultiplier;
                rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

                // Поворот спрайта по направлению движения
                if (Mathf.Abs(direction.x) > 0.01f)
                {
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
                    transform.localScale = scale;
                }
            }
        }

        #endregion

        #region Attack

        private void AttackUpdate()
        {
            // В режиме атаки стоим на месте
            rb.velocity = new Vector2(0f, rb.velocity.y);

            if (player == null)
                return;

            // Смотреть на игрока
            float dirX = Mathf.Sign(player.position.x - transform.position.x);
            Vector3 scale = transform.localScale;
            scale.x = dirX * Mathf.Abs(scale.x);
            transform.localScale = scale;

            _shootTimer += Time.fixedDeltaTime;
            if (_shootTimer >= shootingInterval)
            {
                _shootTimer = 0f;
                ShootAtPlayer();
            }
        }

        private void ApplyWind()
        {
            if (weatherManager == null) return;

            Vector2 force = _mods.windForcePerSecond;
            rb.AddForce(force, ForceMode2D.Force);
        }

        private void ShootAtPlayer()
        {
            if (projectilePrefab == null || shootPoint == null || player == null)
                return;

            // Стреляем только по горизонтали, параллельно полу
            float dirX = Mathf.Sign(player.position.x - shootPoint.position.x);
            if (Mathf.Approximately(dirX, 0f))
            {
                dirX = transform.localScale.x >= 0f ? 1f : -1f;
            }

            Vector2 dir = new Vector2(dirX, 0f);

            GameObject projObj = Object.Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            var projectile = projObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(dir, transform, projectileDamage, projectileSpeed);
            }
        }

        #endregion
    }
}

