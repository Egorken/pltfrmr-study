using UnityEngine;

namespace Game.LevelSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [SerializeField] private Collider2D platformCollider;

        private void Reset()
        {
            platformCollider = GetComponent<Collider2D>();
        }

        private void Awake()
        {
            if (platformCollider == null) platformCollider = GetComponent<Collider2D>();
        }

        public Collider2D GetCollider() => platformCollider != null ? platformCollider : GetComponent<Collider2D>();
    }
}
