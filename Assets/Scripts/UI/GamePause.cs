using System;
using UnityEngine;

namespace Game.UI
{
    public class GamePause : MonoBehaviour
    {
        public static GamePause Instance { get; private set; }

        [Header("Клавиша паузы")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        public bool IsPaused { get; private set; }

        public event Action<bool> OnPauseChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
                TogglePause();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Time.timeScale = 1f;
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
            OnPauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            OnPauseChanged?.Invoke(false);
        }

        public void TogglePause()
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }
}
