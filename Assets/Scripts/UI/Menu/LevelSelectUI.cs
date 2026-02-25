using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Game.UI.Menu
{
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("Scene names уровней (порядок как в Build Settings)")]
        [SerializeField] private List<string> levelSceneNames = new List<string> { "SampleScene" };

        [Header("UI")]
        [SerializeField] private Transform levelButtonsContainer;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(LoadMainMenu);

            if (levelButtonsContainer != null && levelButtonPrefab != null && levelSceneNames.Count > 0)
            {
                for (int i = 0; i < levelSceneNames.Count; i++)
                {
                    string sceneName = levelSceneNames[i];
                    GameObject go = Instantiate(levelButtonPrefab, levelButtonsContainer);
                    var btn = go.GetComponent<Button>();
                    var label = go.GetComponentInChildren<TMP_Text>();
                    if (label != null) label.text = $"Уровень {i + 1}";
                    if (btn != null)
                    {
                        string capture = sceneName;
                        btn.onClick.AddListener(() => LoadLevel(capture));
                    }
                }
            }
        }

        public void LoadLevel(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
                SceneManager.LoadScene(sceneName);
        }

        public void LoadLevelByIndex(int index)
        {
            if (index >= 0 && index < levelSceneNames.Count)
                SceneManager.LoadScene(levelSceneNames[index]);
        }

        public void LoadMainMenu()
        {
            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
