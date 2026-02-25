using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.UI
{
    public class PauseUI : MonoBehaviour
    {
        [Header("Панель и кнопки")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private GamePause _gamePause;

        private void Start()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
            else
                Debug.LogWarning("[PauseUI] Pause Panel не назначена — меню паузы не появится. Перетащи панель в инспектор.", this);

            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);

            _gamePause = GamePause.Instance != null ? GamePause.Instance : FindObjectOfType<GamePause>();
            if (_gamePause != null)
            {
                _gamePause.OnPauseChanged += OnPauseChanged;
                if (_gamePause.IsPaused && pausePanel != null)
                    pausePanel.SetActive(true);
            }
            else
                Debug.LogWarning("[PauseUI] GamePause не найден на сцене — меню паузы не будет реагировать на паузу.", this);
        }

        private void OnDestroy()
        {
            if (_gamePause != null)
                _gamePause.OnPauseChanged -= OnPauseChanged;
        }

        private void OnPauseChanged(bool paused)
        {
            if (pausePanel != null)
                pausePanel.SetActive(paused);
        }

        private void OnResumeClicked()
        {
            GamePause.Instance?.Resume();
        }

        private void OnMenuClicked()
        {
            if (GamePause.Instance != null)
                GamePause.Instance.Resume();
            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
