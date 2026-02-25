using Game.Player;
using UnityEngine;

namespace Game.LevelSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class Ladder : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var controller = other.GetComponent<PlayerController>();
            if (controller != null)
                controller.EnterLadder();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var controller = other.GetComponent<PlayerController>();
            if (controller != null)
                controller.ExitLadder();
        }
    }
}
