using UnityEngine;

namespace Game.Player
{
    public class PlayerRunState : PlayerState
    {
        public PlayerRunState(PlayerController player, PlayerStateMachine stateMachine)
            : base(player, stateMachine) { }

        public override void LogicUpdate()
        {
            if (!player.IsGrounded)
            {
                stateMachine.ChangeState(player.FallState);
                return;
            }

            if (Mathf.Abs(player.InputX) < 0.01f)
            {
                stateMachine.ChangeState(player.IdleState);
                return;
            }

            if (player.JumpPressed && player.IsGrounded)
            {
                stateMachine.ChangeState(player.JumpState);
            }
        }

        public override void PhysicsUpdate()
        {
            player.MoveHorizontally(player.InputX);
        }
    }
}

