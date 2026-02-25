using System.Collections.Generic;
using Game.NPC;
using Game.Quest;
using UnityEngine;

namespace Game.LevelSystem
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class QuestDoorPlatform : MonoBehaviour
    {
        [Header("Квест")]
        [SerializeField] private TalkableNPC openOnlyWhenThisNpcQuestCompleted;

        [Header("Позиции")]
        [SerializeField] private Transform startPosition;
        [SerializeField] private Transform endPosition;

        [Header("Движение")]
        [SerializeField] private float speed = 2f;
        [SerializeField] private float reachThreshold = 0.05f;

        private Rigidbody2D _rb;
        private Vector2 _startPos;
        private Vector2 _endPos;
        private bool _triggered;
        private bool _reached;
        private Vector2 _previousPosition;
        private readonly HashSet<Rigidbody2D> _riders = new HashSet<Rigidbody2D>();
        private QuestManager _questManager;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb != null) _rb.bodyType = RigidbodyType2D.Kinematic;

            _startPos = startPosition != null ? (Vector2)startPosition.position : (Vector2)transform.position;
            _endPos = endPosition != null ? (Vector2)endPosition.position : _startPos;

            transform.position = _startPos;
            if (_rb != null) _rb.position = _startPos;
        }

        private void Start()
        {
            _questManager = QuestManager.Instance ?? FindObjectOfType<QuestManager>();
            if (_questManager != null)
                _questManager.OnQuestCompleted += OnQuestCompleted;
        }

        private void OnCollisionEnter2D(Collision2D col) => TryAddRider(col);
        private void OnCollisionStay2D(Collision2D col) => TryAddRider(col);
        private void OnCollisionExit2D(Collision2D col)
        {
            if (col.rigidbody != null) _riders.Remove(col.rigidbody);
        }

        private void TryAddRider(Collision2D col)
        {
            if (col.rigidbody == null || col.rigidbody.bodyType == RigidbodyType2D.Static) return;
            foreach (ContactPoint2D c in col.contacts)
            {
                if (c.normal.y >= 0.5f || c.normal.y <= -0.5f) { _riders.Add(col.rigidbody); break; }
            }
            float myY = _rb != null ? _rb.position.y : transform.position.y;
            if (col.rigidbody.position.y > myY + 0.1f) _riders.Add(col.rigidbody);
        }

        private void OnDisable()
        {
            if (_questManager != null)
                _questManager.OnQuestCompleted -= OnQuestCompleted;
        }

        private void OnQuestCompleted(TalkableNPC completedQuestGiver)
        {
            if (openOnlyWhenThisNpcQuestCompleted != null && completedQuestGiver != openOnlyWhenThisNpcQuestCompleted)
                return;
            if (endPosition == null) return;
            _triggered = true;
        }

        private void FixedUpdate()
        {
            Vector2 current = _rb != null ? _rb.position : (Vector2)transform.position;
            _previousPosition = current;

            if (!_triggered || _reached)
            {
                MoveRiders(Vector2.zero);
                return;
            }

            Vector2 toEnd = _endPos - current;
            float distance = toEnd.magnitude;

            if (distance < reachThreshold)
            {
                if (_rb != null) _rb.position = _endPos;
                else transform.position = _endPos;
                MoveRiders(_endPos - _previousPosition);
                _reached = true;
                return;
            }

            Vector2 move = toEnd.normalized * (speed * Time.fixedDeltaTime);
            Vector2 newPos = move.sqrMagnitude >= distance * distance ? _endPos : current + move;
            if (_rb != null) _rb.MovePosition(newPos);
            else transform.position = newPos;
            MoveRiders(newPos - _previousPosition);
        }

        private void MoveRiders(Vector2 delta)
        {
            if (delta.sqrMagnitude < 1e-10f) return;
            foreach (Rigidbody2D r in new List<Rigidbody2D>(_riders))
            {
                if (r != null && !r.Equals(null)) r.position += delta;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 start = startPosition != null ? startPosition.position : transform.position;
            Vector2 end = endPosition != null ? endPosition.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(start, 0.2f);
            Gizmos.DrawWireSphere(end, 0.2f);
            Gizmos.DrawLine(start, end);
        }
    }
}
