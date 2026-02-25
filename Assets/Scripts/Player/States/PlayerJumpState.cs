using UnityEngine;

namespace Game.Player
{
    public class PlayerJumpState : PlayerState
    {
        public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine)
            : base(player, stateMachine) { }

        public override void Enter() { }

        public override void LogicUpdate()
        {
            // variable jump height
            if (player.JumpReleased)
            {
                player.ApplyJumpCut();
            }

            // когда начинаем падать – в Fall
            if (player.VerticalVelocity <= 0f)
            {
                stateMachine.ChangeState(player.FallState);
            }
        }

        public override void PhysicsUpdate()
        {
            // управление в воздухе
            player.MoveHorizontally(player.InputX);
        }
    }
}

