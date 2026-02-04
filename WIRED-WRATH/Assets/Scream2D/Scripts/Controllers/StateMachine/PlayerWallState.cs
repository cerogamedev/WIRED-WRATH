using UnityEngine;

namespace Scream2D.Controllers.StateMachine
{
    public class PlayerWallState : PlayerBaseState
    {
        public PlayerWallState(PlayerController currentContext, PlayerStateMachine playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.CurrentAirJumps = 0; // Reset Double Jump
            _ctx.CanDash = true;      // Reset Dash Charge
            // Optional: Dust particles
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void FixedUpdateState()
        {
            // Wall Slide Logic
            if (_ctx.GetVelocity().y < -_ctx.WallSlideSpeed)
            {
                _ctx.SetVelocity(new Vector2(_ctx.GetVelocity().x, -_ctx.WallSlideSpeed));
            }
        }

        public override void ExitState()
        {
        }

        public override void CheckSwitchStates()
        {
            // Wall Jump
            if (_ctx.IsJumpPressed)
            {
                WallJump();
                return; 
            }
            
            if (_ctx.IsGrabHeld && _ctx.CurrentStamina > 0)
            {
                SwitchState(_factory.ClimbState);
            }

            if (_ctx.IsGrounded)
            {
                SwitchState(_factory.GroundedState);
            }
            else if (!_ctx.IsTouchingWall || _ctx.MoveInput.x != _ctx.FacingDirection) // Leaving wall
            {
                SwitchState(_factory.FallState);
            }
        }

        public override void InitializeSubState() { }
        
        private void WallJump()
        {
            // Jump away from wall
            Vector2 force = new Vector2(-_ctx.FacingDirection * _ctx.WallJumpForce.x, _ctx.WallJumpForce.y);
            _ctx.SetVelocity(Vector2.zero); // Reset to ensure consistent force
            _ctx.AddForce(force, ForceMode2D.Impulse);
            
            _ctx.ConsumeJumpInput();
            _ctx.FlipSprite(_ctx.FacingDirection < 0); // Face jump direction
            
            // Switch to fall immediately? Or specialized Air state? FallState checks inputs.
            // If we switch to FallState, it will apply air control.
            // Ideally Disable air control for a split second (Wall Jump Lock).
            // For now, let's just switch to FallState.
            SwitchState(_factory.FallState);
        }
    }
}
