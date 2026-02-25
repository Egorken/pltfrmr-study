using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.LevelSystem
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Levels")]
        [SerializeField] private List<string> levelSceneNames = new List<string>();

        [Header("Player")]
        [SerializeField] private Transform player;

        private int _currentLevelIndex;
        private Vector3 _lastCheckpointPosition;
        private bool _hasCheckpoint;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null)
                transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void Start()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            _currentLevelIndex = levelSceneNames.IndexOf(activeScene);
            if (_currentLevelIndex < 0)
                _currentLevelIndex = 0;

            if (player != null)
            {
                _lastCheckpointPosition = player.position;
                _hasCheckpoint = true;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Найти игрока и опционально телепортировать на чекпоинт этой сцены
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }

            if (_hasCheckpoint && player != null)
            {
                player.position = _lastCheckpointPosition;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;
            }
        }

        // --- API ---

        public bool HasCheckpoint() => _hasCheckpoint;

        public void RegisterCheckpoint(Vector3 position)
        {
            _lastCheckpointPosition = position;
            _hasCheckpoint = true;
        }

        public void RespawnPlayer()
        {
            if (!_hasCheckpoint) return;
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
            }
            if (player == null) return;

            player.position = _lastCheckpointPosition;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }

        public void LoadLevelByName(string sceneName)
        {
            int index = levelSceneNames.IndexOf(sceneName);
            if (index >= 0)
            {
                _currentLevelIndex = index;
                SceneManager.LoadScene(sceneName);
            }
        }

        public void ReloadCurrentLevel()
        {
            if (levelSceneNames.Count == 0) return;

            string sceneName = levelSceneNames[_currentLevelIndex];
            SceneManager.LoadScene(sceneName);
        }

        public void LoadNextLevel()
        {
            if (levelSceneNames.Count == 0) return;

            _currentLevelIndex = (_currentLevelIndex + 1) % levelSceneNames.Count;
            string sceneName = levelSceneNames[_currentLevelIndex];
            SceneManager.LoadScene(sceneName);

            _hasCheckpoint = false; // новый уровень — новые чекпоинты
        }
    }
}

