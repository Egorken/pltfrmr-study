using Game.Enemy;
using UnityEngine;

namespace Game.Quest
{
    [RequireComponent(typeof(EnemyHealth))]
    public class QuestTargetBoss : MonoBehaviour
    {
        private void Awake()
        {
            var health = GetComponent<EnemyHealth>();
            if (health != null)
                health.OnDeath += OnBossDied;
        }

        private void OnBossDied()
        {
            if (QuestManager.Instance != null && QuestManager.Instance.IsKillBossQuestFor(gameObject))
                QuestManager.Instance.CompleteQuest();
        }
    }
}
