using UnityEngine;

namespace Game.CameraSystem
{
    public class PlatformerCamera2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.2f;

        [Header("Look Ahead")]
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeedThreshold = 0.1f;
        [SerializeField] private float lookAheadReturnSpeed = 4f;

        private Vector3 _currentVelocity;
        private Vector3 _lookAheadPos;
        private float _lastTargetX;
        private Rigidbody2D _targetRb;

        private void Start()
        {
            if (target != null)
            {
                _lastTargetX = target.position.x;
                _targetRb = target.GetComponent<Rigidbody2D>();
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float targetX = target.position.x;
            float deltaX = targetX - _lastTargetX;

            float velX = _targetRb != null ? _targetRb.velocity.x : deltaX / Mathf.Max(Time.deltaTime, 0.0001f);

            bool moving = Mathf.Abs(velX) > lookAheadSpeedThreshold;
            if (moving)
            {
                _lookAheadPos = new Vector3(
                    Mathf.Sign(velX) * lookAheadDistance,
                    0f,
                    0f
                );
            }
            else
            {
                _lookAheadPos = Vector3.MoveTowards(
                    _lookAheadPos,
                    Vector3.zero,
                    lookAheadReturnSpeed * Time.deltaTime
                );
            }

            _lastTargetX = targetX;

            Vector3 targetPos = target.position + offset + _lookAheadPos;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, smoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _targetRb = target != null ? target.GetComponent<Rigidbody2D>() : null;
        }
    }
}

