using UnityEngine;

namespace Game.LevelSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Vector3 point = spawnPoint != null ? spawnPoint.position : transform.position;
            LevelManager.Instance?.RegisterCheckpoint(point);
        }
    }
}

