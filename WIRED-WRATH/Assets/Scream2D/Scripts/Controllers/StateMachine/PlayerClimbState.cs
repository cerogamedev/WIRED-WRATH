using UnityEngine;

namespace Scream2D.Controllers.StateMachine
{
    public class PlayerClimbState : PlayerBaseState
    {
        public PlayerClimbState(PlayerController currentContext, PlayerStateMachine playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.IsGrounded = false;
            _ctx.SetVelocity(Vector2.zero);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            
            // Stamina System
            _ctx.CurrentStamina -= Time.deltaTime * 20f; // Drain rate
            
            // Movement
            float y = _ctx.MoveInput.y;
            _ctx.SetVelocity(new Vector2(0, y * _ctx.WallClimbSpeed));
        }

        public override void FixedUpdateState()
        {
        }

        public override void ExitState()
        {
            // Reset gravity or anything if modified
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.IsJumpPressed)
            {
               // Wall Jump from Climb
               // Logic similar to WallState but maybe distinct launch
               _factory.WallState.EnterState(); // Use WallState logic for jump? 
               // Or perform jump here. Let's redirect to WallState logic which has the jump.
               // Actually we need to call Jump logic.
               // Let's implement WallJump here too or move to shared method.
               PerformClimbJump();
               return;
            }
            
            if (!_ctx.IsGrabHeld || !_ctx.IsTouchingWall || _ctx.CurrentStamina <= 0)
            {
                SwitchState(_factory.FallState);
            }
            else if (_ctx.IsGrounded) // Reached floor
            {
                SwitchState(_factory.GroundedState);
            }
        }

        public override void InitializeSubState() { }
        
        private void PerformClimbJump()
        {
             // Jump away or up? Celeste allows climbing jump (up) or wall kick (away).
             // If holding UP, jump UP. If holding AWAY, jump AWAY. Neutral = Away.
             
             Vector2 force = Vector2.zero;
             
             if (_ctx.MoveInput.y > 0) // Climbing Up Jump
             {
                 force = new Vector2(0, _ctx.WallJumpForce.y * 1.2f); // Emphasize up
             }
             else // Wall Kick
             {
                 force = new Vector2(-_ctx.FacingDirection * _ctx.WallJumpForce.x, _ctx.WallJumpForce.y);
                 _ctx.FlipSprite(_ctx.FacingDirection < 0);
             }
             
             _ctx.SetVelocity(Vector2.zero);
             _ctx.AddForce(force, ForceMode2D.Impulse);
             _ctx.ConsumeJumpInput();
             SwitchState(_factory.FallState);
        }
    }
}
