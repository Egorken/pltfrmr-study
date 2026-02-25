using UnityEngine;

namespace Game.LevelSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class LevelPortal : MonoBehaviour
    {
        [SerializeField] private string targetSceneName;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            if (LevelManager.Instance == null) return;

            if (!string.IsNullOrEmpty(targetSceneName))
                LevelManager.Instance.LoadLevelByName(targetSceneName);
            else
                LevelManager.Instance.LoadNextLevel();
        }
    }
}

