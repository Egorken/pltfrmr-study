using System.Collections.Generic;
using UnityEngine;

namespace Game.LevelSystem
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Точки пути")]
        [SerializeField] private Transform[] waypoints = new Transform[0];

        [Header("Движение")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private float reachThreshold = 0.05f;
        [SerializeField] private float waitAtWaypoint = 0f;

        public enum PathMode { Loop, PingPong }

        [SerializeField] private PathMode pathMode = PathMode.Loop;
        [SerializeField] private bool debugRiders;

        private Rigidbody2D _rb;
        private int _currentIndex;
        private int _direction = 1;
        private float _waitTimer;
        private Vector2 _previousPosition;
        private readonly HashSet<Rigidbody2D> _riders = new HashSet<Rigidbody2D>();

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb != null) _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Start()
        {
            if (waypoints == null || waypoints.Length < 2)
                return;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                    return;
            }

            Vector2 startPos = waypoints[0].position;
            transform.position = startPos;
            if (_rb != null) _rb.position = startPos;
            _currentIndex = 0;
        }

        private void FixedUpdate()
        {
            Vector2 current = _rb != null ? _rb.position : (Vector2)transform.position;
            _previousPosition = current;

            if (waypoints == null || waypoints.Length < 2)
            {
                MoveRiders(Vector2.zero);
                return;
            }

            Transform target = waypoints[_currentIndex];
            if (target == null)
            {
                MoveRiders(Vector2.zero);
                return;
            }

            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.fixedDeltaTime;
                MoveRiders(Vector2.zero);
                return;
            }

            Vector2 toTarget = (Vector2)target.position - current;
            float distance = toTarget.magnitude;

            if (distance < reachThreshold)
            {
                Vector2 pos = target.position;
                if (_rb != null) _rb.MovePosition(pos);
                else transform.position = pos;
                MoveRiders(pos - _previousPosition);
                _waitTimer = waitAtWaypoint;
                AdvanceToNextWaypoint();
                return;
            }

            Vector2 move = toTarget.normalized * (speed * Time.fixedDeltaTime);
            Vector2 newPos = move.sqrMagnitude >= distance * distance ? (Vector2)target.position : current + move;
            if (_rb != null) _rb.MovePosition(newPos);
            else transform.position = newPos;
            MoveRiders(newPos - _previousPosition);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            TryAddRider(col);
        }

        private void OnCollisionStay2D(Collision2D col)
        {
            TryAddRider(col);
        }

        private void OnCollisionExit2D(Collision2D col)
        {
            if (col.rigidbody != null)
            {
                if (_riders.Remove(col.rigidbody) && debugRiders)
                    Debug.Log($"[MovingPlatform] Убран пассажир: {col.gameObject.name}", this);
            }
        }

        private void TryAddRider(Collision2D col)
        {
            if (col.rigidbody == null || col.rigidbody.bodyType == RigidbodyType2D.Static) return;

            // Вариант 1: контакт сверху по нормали (нормаль в Unity может быть от "входящего" коллайдера — тогда у стоящего сверху normal.y < 0)
            foreach (ContactPoint2D contact in col.contacts)
            {
                if (contact.normal.y >= 0.5f || contact.normal.y <= -0.5f)
                {
                    if (_riders.Add(col.rigidbody) && debugRiders)
                        Debug.Log($"[MovingPlatform] Добавлен пассажир: {col.gameObject.name}", this);
                    return;
                }
            }

            // Вариант 2: объект явно выше центра платформы (стоит на нас)
            float myY = _rb != null ? _rb.position.y : transform.position.y;
            float otherY = col.rigidbody.position.y;
            if (otherY > myY + 0.1f && _riders.Add(col.rigidbody) && debugRiders)
                Debug.Log($"[MovingPlatform] Добавлен пассажир (по высоте): {col.gameObject.name}", this);
        }

        private void MoveRiders(Vector2 delta)
        {
            if (delta.sqrMagnitude < 1e-10f) return;
            foreach (Rigidbody2D rider in new List<Rigidbody2D>(_riders))
            {
                if (rider != null && !rider.Equals(null))
                    rider.position += delta;
            }
        }

        private void AdvanceToNextWaypoint()
        {
            if (pathMode == PathMode.Loop)
            {
                _currentIndex = (_currentIndex + 1) % waypoints.Length;
            }
            else
            {
                _currentIndex += _direction;
                if (_currentIndex >= waypoints.Length - 1) { _currentIndex = waypoints.Length - 1; _direction = -1; }
                else if (_currentIndex <= 0) { _currentIndex = 0; _direction = 1; }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null) return;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

                if (pathMode == PathMode.Loop && i < waypoints.Length - 1)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                if (pathMode == PathMode.Loop && i == waypoints.Length - 1 && waypoints.Length > 1)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                if (pathMode == PathMode.PingPong && i < waypoints.Length - 1)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
