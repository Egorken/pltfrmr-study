using Game.Inventory;
using Game.Quest;
using TMPro;
using UnityEngine;

namespace Game.NPC
{
    [RequireComponent(typeof(Collider2D))]
    public class TalkableNPC : MonoBehaviour
    {
        [Header("Диалог")]
        [SerializeField] private string[] dialogLines = { "Привет, путник!", "Рад тебя видеть." };
        [SerializeField] private string[] postQuestDialogLines = { "Можешь идти дальше.", "Спасибо за помощь!" };
        [SerializeField] private float autoAdvanceAfter = 0f;

        [Header("Квест")]
        [SerializeField] private bool giveQuestAfterDialog;
        [SerializeField] private QuestType questType = QuestType.Fetch;
        [SerializeField, TextArea(1, 2)] private string questDescription = "Найди ключ и принеси его мне.";
        [SerializeField] private ItemType fetchItemType = ItemType.QuestKey;
        [SerializeField] private GameObject killBossTarget;
        [SerializeField] private string completionLine = "Спасибо! Задание выполнено.";

        [Header("Взаимодействие")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode skipKey = KeyCode.Escape;
        [SerializeField] private GameObject dialogBubble;
        [SerializeField] private TMP_Text dialogTextField;
        [SerializeField] private string playerTag = "Player";

        private bool _playerInRange;
        private bool _dialogVisible;
        private int _currentLineIndex;
        private float _autoAdvanceTimer;
        private Transform _playerTransform;
        private bool _showingCompletion;
        private bool _questCompletedByThisNpc;
        private string[] _currentLines;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void Awake()
        {
            if (dialogBubble == null)
                dialogBubble = FindDialogBubbleInChildren();
            if (dialogTextField == null && dialogBubble != null)
                dialogTextField = dialogBubble.GetComponentInChildren<TMP_Text>(true);

            if (dialogBubble != null)
                dialogBubble.SetActive(false);
        }

        private GameObject FindDialogBubbleInChildren()
        {
            var canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null) return canvas.gameObject;
            var tmp = GetComponentInChildren<TMP_Text>(true);
            return tmp != null ? tmp.gameObject : null;
        }

        private void Update()
        {
            if (_dialogVisible)
            {
                if (Input.GetKeyDown(skipKey))
                {
                    HideDialog();
                    return;
                }
                if (autoAdvanceAfter > 0f)
                {
                    _autoAdvanceTimer -= Time.deltaTime;
                    if (_autoAdvanceTimer <= 0f)
                    {
                        AdvanceOrClose();
                        _autoAdvanceTimer = autoAdvanceAfter;
                    }
                }
                if (Input.GetKeyDown(interactKey))
                {
                    AdvanceOrClose();
                    if (_dialogVisible)
                        _autoAdvanceTimer = autoAdvanceAfter;
                }
                return;
            }

            if (_playerInRange && Input.GetKeyDown(interactKey))
                TryShowDialog();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
            {
                _playerInRange = true;
                _playerTransform = other.transform;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
            {
                _playerInRange = false;
                _playerTransform = null;
                if (_dialogVisible)
                    HideDialog();
            }
        }

        private QuestManager GetQuestManager() =>
            QuestManager.Instance ?? UnityEngine.Object.FindObjectOfType<QuestManager>();

        private void TryShowDialog()
        {
            var inventory = _playerTransform != null ? _playerTransform.GetComponent<PlayerInventory>() : null;
            var qm = GetQuestManager();

            if (qm != null && qm.IsFetchQuestFor(this) && inventory != null && inventory.HasItem(fetchItemType))
            {
                _showingCompletion = true;
                _currentLines = new[] { completionLine };
                ShowDialogWithLines(_currentLines);
                return;
            }

            _showingCompletion = false;
            bool usePostQuest = UsePostQuestDialog();
            string[] lines = usePostQuest ? GetPostQuestLines() : dialogLines;
            if (lines == null || lines.Length == 0) return;
            _currentLines = lines;
            ShowDialogWithLines(lines);
        }

        private bool UsePostQuestDialog()
        {
            if (!giveQuestAfterDialog) return false;
            if (_questCompletedByThisNpc) return true;
            if (questType == QuestType.KillBoss && killBossTarget == null) return true;
            return false;
        }

        private string[] GetPostQuestLines()
        {
            if (postQuestDialogLines != null && postQuestDialogLines.Length > 0)
                return postQuestDialogLines;
            return dialogLines;
        }

        private void ShowDialogWithLines(string[] lines)
        {
            if (lines == null || lines.Length == 0) return;

            _currentLineIndex = 0;
            if (dialogTextField != null && lines.Length > 0)
                dialogTextField.text = lines[0];
            if (dialogBubble != null)
                dialogBubble.SetActive(true);

            _dialogVisible = true;
            if (autoAdvanceAfter > 0f)
                _autoAdvanceTimer = autoAdvanceAfter;
        }

        private void AdvanceOrClose()
        {
            string[] lines = _currentLines;
            if (lines == null || lines.Length == 0) return;

            _currentLineIndex++;
            if (_currentLineIndex >= lines.Length)
            {
                if (_showingCompletion)
                {
                    var inventory = _playerTransform != null ? _playerTransform.GetComponent<PlayerInventory>() : null;
                    if (inventory != null) inventory.TryRemoveItem(fetchItemType);
                    GetQuestManager()?.CompleteQuest();
                    _questCompletedByThisNpc = true;
                }
                else if (giveQuestAfterDialog)
                {
                    var qm = GetQuestManager();
                    if (qm != null && qm.CurrentType == QuestType.None && !_questCompletedByThisNpc)
                    {
                        if (questType == QuestType.Fetch)
                            qm.GiveFetchQuest(questDescription, this, fetchItemType);
                        else if (questType == QuestType.KillBoss && killBossTarget != null)
                            qm.GiveKillBossQuest(questDescription, killBossTarget, this);
                    }
                }
                HideDialog();
                return;
            }
            if (dialogTextField != null && _currentLineIndex < lines.Length)
                dialogTextField.text = lines[_currentLineIndex];
        }

        private void SetCurrentLineText()
        {
            if (dialogTextField == null || _currentLines == null || _currentLineIndex < 0 || _currentLineIndex >= _currentLines.Length)
                return;
            dialogTextField.text = _currentLines[_currentLineIndex];
        }

        private void HideDialog()
        {
            if (dialogBubble != null)
                dialogBubble.SetActive(false);
            _dialogVisible = false;
            _currentLineIndex = 0;
            _showingCompletion = false;
        }
    }
}
