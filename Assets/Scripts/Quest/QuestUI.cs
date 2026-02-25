using TMPro;
using UnityEngine;

namespace Game.Quest
{
    public class QuestUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private string titleLabel = "Текущее задание:";
        [SerializeField] private TMP_Text objectiveText;

        private QuestManager _questManager;

        private void Start()
        {
            if (titleText != null)
                titleText.text = titleLabel;
            _questManager = QuestManager.Instance ?? UnityEngine.Object.FindObjectOfType<QuestManager>();
            Refresh();
            if (_questManager != null)
                _questManager.OnQuestChanged += OnQuestChanged;
        }

        private void OnDestroy()
        {
            if (_questManager != null)
                _questManager.OnQuestChanged -= OnQuestChanged;
        }

        private void OnQuestChanged(QuestState _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (objectiveText == null) return;
            QuestManager qm = _questManager ?? QuestManager.Instance ?? UnityEngine.Object.FindObjectOfType<QuestManager>();
            objectiveText.text = qm != null ? qm.GetDisplayText() : "Исследуйте местность";
        }
    }
}
