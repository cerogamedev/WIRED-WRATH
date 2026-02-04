using UnityEngine;

namespace Scream2D.Controllers.StateMachine
{
    public class PlayerFallState : PlayerBaseState
    {
        public PlayerFallState(PlayerController currentContext, PlayerStateMachine playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.PlayAnimation("JumpDown");
            _ctx.SetGravityScale(_ctx.FallGravityMult);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void FixedUpdateState()
        {
            // Air Control
            float targetSpeed = _ctx.MoveInput.x * _ctx.MoveSpeed;
            Vector2 currentVel = _ctx.GetVelocity();
            float newX = Mathf.MoveTowards(currentVel.x, targetSpeed, (_ctx.Acceleration * _ctx.AirControl) * Time.fixedDeltaTime);
            
            _ctx.SetVelocity(new Vector2(newX, currentVel.y));
            
            if (Mathf.Abs(newX) > 0.1f)
            {
                _ctx.FlipSprite(newX > 0);
            }

            // Gravity Multiplier Logic (Fast Fall vs Apex)
            if (Mathf.Abs(_ctx.GetVelocity().y) < _ctx.JumpApexThreshold)
            {
                _ctx.SetGravityScale(_ctx.JumpApexGravityMult);
            }
            else
            {
                _ctx.SetGravityScale(_ctx.FallGravityMult);
            }
        }

        public override void ExitState()
        {
            _ctx.ApplySquashAndStretch();
            _ctx.SetGravityScale(1f);
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.IsGrounded)
            {
                SwitchState(_factory.GroundedState);
                return;
            }

            // Coyote Time
            if (_ctx.IsJumpPressed && (Time.time - _ctx.LastGroundedTime) < _ctx.CoyoteTime)
            {
                SwitchState(_factory.JumpState);
                return;
            }

            // Wall Interactions
            if (_ctx.IsTouchingWall)
            {
                if (_ctx.IsGrabHeld && _ctx.CurrentStamina > 0)
                {
                    SwitchState(_factory.ClimbState);
                    return;
                }
                
                if (_ctx.MoveInput.x == _ctx.FacingDirection && _ctx.GetVelocity().y < 0)
                {
                    SwitchState(_factory.WallState);
                    return;
                }
            }

            if (_ctx.IsDashing)
            {
                SwitchState(_factory.DashState);
                return;
            }
        }

        public override void InitializeSubState() { }
    }
}
