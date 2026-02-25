using System;
using Game.Inventory;
using Game.NPC;
using UnityEngine;

namespace Game.Quest
{
    public enum QuestType
    {
        None,
        Fetch,
        KillBoss
    }

    [Serializable]
    public class QuestState
    {
        public QuestType Type;
        public string Description;
        public TalkableNPC FetchTargetNpc;
        public ItemType FetchItemType;
        public GameObject KillBossTarget;
        public TalkableNPC QuestGiver;
    }

    [DefaultExecutionOrder(-200)]
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        private const string NoQuestText = "Исследуйте местность";

        [SerializeField] private string noQuestDisplayText = NoQuestText;

        private QuestState _current = new QuestState { Type = QuestType.None, Description = "" };

        public QuestType CurrentType => _current.Type;
        public QuestState CurrentQuest => _current;

        public event Action<QuestState> OnQuestChanged;
        public event Action<TalkableNPC> OnQuestCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public string GetDisplayText()
        {
            if (_current.Type == QuestType.None || string.IsNullOrEmpty(_current.Description))
                return noQuestDisplayText;
            return _current.Description;
        }

        public void GiveFetchQuest(string description, TalkableNPC bringToNpc, ItemType itemType)
        {
            _current = new QuestState
            {
                Type = QuestType.Fetch,
                Description = description,
                FetchTargetNpc = bringToNpc,
                FetchItemType = itemType,
                QuestGiver = bringToNpc
            };
            OnQuestChanged?.Invoke(_current);
        }

        public void GiveKillBossQuest(string description, GameObject boss, TalkableNPC giver)
        {
            _current = new QuestState
            {
                Type = QuestType.KillBoss,
                Description = description,
                KillBossTarget = boss,
                QuestGiver = giver
            };
            OnQuestChanged?.Invoke(_current);
        }

        public void CompleteQuest()
        {
            TalkableNPC completedGiver = _current.QuestGiver;
            _current = new QuestState { Type = QuestType.None, Description = "" };
            OnQuestChanged?.Invoke(_current);
            OnQuestCompleted?.Invoke(completedGiver);
        }

        public bool IsFetchQuestFor(TalkableNPC npc) =>
            _current.Type == QuestType.Fetch && _current.FetchTargetNpc == npc;

        public bool IsKillBossQuestFor(GameObject boss) =>
            _current.Type == QuestType.KillBoss && _current.KillBossTarget == boss;
    }
}
