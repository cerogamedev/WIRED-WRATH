using UnityEngine;

namespace Scream2D.Controllers.StateMachine
{
    public class PlayerStateMachine
    {
        public PlayerBaseState CurrentState { get; set; }
        
        public PlayerGroundedState GroundedState { get; set; }
        public PlayerJumpState JumpState { get; set; }
        public PlayerFallState FallState { get; set; }
        public PlayerWallState WallState { get; set; }
        public PlayerDashState DashState { get; set; }
        public PlayerClimbState ClimbState { get; set; }

        public void Initialize(PlayerBaseState startingState)
        {
            CurrentState = startingState;
            CurrentState.EnterState();
        }

        public void Update()
        {
            CurrentState?.UpdateState();
            CurrentState?.CheckSwitchStates();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdateState();
        }
    }
}
