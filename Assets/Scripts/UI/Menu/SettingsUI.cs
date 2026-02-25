using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI.Menu
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Навигация")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private Button backButton;

        private void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(LoadMainMenu);
        }

        public void LoadMainMenu()
        {
            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
