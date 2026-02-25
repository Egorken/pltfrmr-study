using UnityEngine;

namespace Game.Player
{
    public class PlayerFallState : PlayerState
    {
        public PlayerFallState(PlayerController player, PlayerStateMachine stateMachine)
            : base(player, stateMachine) { }

        public override void LogicUpdate()
        {
            if (player.IsGrounded)
            {
                if (Mathf.Abs(player.InputX) > 0.01f)
                    stateMachine.ChangeState(player.RunState);
                else
                    stateMachine.ChangeState(player.IdleState);
                return;
            }
            if (player.JumpPressed && player.AirJumpsLeft > 0)
                stateMachine.ChangeState(player.JumpState);
        }

        public override void PhysicsUpdate()
        {
            // воздушный контроль
            player.MoveHorizontally(player.InputX);
        }
    }
}

