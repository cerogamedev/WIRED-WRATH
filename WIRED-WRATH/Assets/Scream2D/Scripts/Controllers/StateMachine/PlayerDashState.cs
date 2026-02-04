using UnityEngine;

namespace Scream2D.Controllers.StateMachine
{
    public class PlayerDashState : PlayerBaseState
    {
        private float _startTime;

        public PlayerDashState(PlayerController currentContext, PlayerStateMachine playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _startTime = Time.time;
            _ctx.IsDashing = true;
            _ctx.CanDash = false;          // Consume dash charge
            _ctx.LastDashTime = Time.time; // Consumed dash
            _ctx.SetGravityScale(0f); // No gravity during dash
            
            _ctx.StartGhostTrail();
            
            // 8-Directional Logic
            Vector2 dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (dashDir == Vector2.zero) dashDir = new Vector2(_ctx.FacingDirection, 0); // Default forward
            
            _ctx.SetVelocity(dashDir * _ctx.DashSpeed);
            
            // Hit Stop / Freeze Frame
            _ctx.FreezeFrame(0.05f);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void FixedUpdateState()
        {
            // Velocity might degrade or stay constant?
            // Celeste dash is constant speed then drops off.
            // For now, constant.
        }

        public override void ExitState()
        {
            _ctx.IsDashing = false;
            _ctx.StopGhostTrail();
            _ctx.SetVelocity(Vector2.zero); 
            _ctx.LastDashTime = Time.time;
            _ctx.DashEndTime = Time.time; // Mark for wave dash
            _ctx.SetGravityScale(1f);
        }

        public override void CheckSwitchStates()
        {
            // Interrupt dash if we hit a wall
            if (_ctx.IsTouchingWall)
            {
                if (_ctx.IsGrabHeld && _ctx.CurrentStamina > 0)
                {
                    SwitchState(_factory.ClimbState);
                    return;
                }
                
                // If we are moving towards the wall, enter wall state
                if (_ctx.MoveInput.x == _ctx.FacingDirection)
                {
                    SwitchState(_factory.WallState);
                    return;
                }
            }

            if (Time.time - _startTime > _ctx.DashDuration)
            {
                if (_ctx.IsGrounded)
                {
                    SwitchState(_factory.GroundedState);
                }
                else
                {
                   SwitchState(_factory.FallState);
                }
            }
        }

        public override void InitializeSubState() { }
    }
}
