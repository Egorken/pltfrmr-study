using UnityEngine;

namespace Game.Player
{
    public abstract class PlayerState
    {
        protected readonly PlayerController player;
        protected readonly PlayerStateMachine stateMachine;

        protected PlayerState(PlayerController player, PlayerStateMachine stateMachine)
        {
            this.player = player;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }

        public virtual void HandleInput() { }
        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }
    }
}

