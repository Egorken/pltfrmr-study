using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI.Menu
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Scene names (как в Build Settings)")]
        [SerializeField] private string levelSelectSceneName = "LevelSelect";
        [SerializeField] private string settingsSceneName = "Settings";

        [Header("Buttons (опционально — можно вызывать методы из OnClick)")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(LoadLevelSelect);
            if (settingsButton != null) settingsButton.onClick.AddListener(LoadSettings);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
        }

        public void LoadLevelSelect()
        {
            if (!string.IsNullOrEmpty(levelSelectSceneName))
                SceneManager.LoadScene(levelSelectSceneName);
        }

        public void LoadSettings()
        {
            if (!string.IsNullOrEmpty(settingsSceneName))
                SceneManager.LoadScene(settingsSceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
