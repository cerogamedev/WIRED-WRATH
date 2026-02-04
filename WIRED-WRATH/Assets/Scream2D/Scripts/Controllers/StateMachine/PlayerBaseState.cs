namespace Scream2D.Controllers.StateMachine
{
    public abstract class PlayerBaseState
    {
        protected PlayerController _ctx;
        protected PlayerStateMachine _factory;

        public PlayerBaseState(PlayerController currentContext, PlayerStateMachine playerStateFactory)
        {
            _ctx = currentContext;
            _factory = playerStateFactory;
        }

        public abstract void EnterState();

        public abstract void UpdateState();

        public abstract void FixedUpdateState();

        public abstract void ExitState();

        public abstract void CheckSwitchStates();

        public abstract void InitializeSubState();

        protected void SwitchState(PlayerBaseState newState)
        {
            // Exit current state
            ExitState();

            // Enter new state
            newState.EnterState();
            
            // Update machine's state
            _factory.CurrentState = newState;
        }
    }
}
