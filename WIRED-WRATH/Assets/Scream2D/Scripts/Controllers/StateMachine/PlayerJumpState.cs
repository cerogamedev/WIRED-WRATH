using UnityEngine;

namespace Scream2D.Controllers.StateMachine
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(PlayerController currentContext, PlayerStateMachine playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.PlayAnimation("JumpUp");
            HandleJump();
            _ctx.PlayJumpDust();
            _ctx.SetGravityScale(_ctx.UpwardGravityMult); // Slow Rise
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            
            // Variable Jump Height (Cut velocity if released)
            if (!_ctx.IsJumpHeld && _ctx.GetVelocity().y > 0)
            {
                _ctx.SetVelocity(new Vector2(_ctx.GetVelocity().x, _ctx.GetVelocity().y * _ctx.VariableJumpHeightMultiplier));
            }
            
            if (Mathf.Abs(_ctx.GetVelocity().y) < _ctx.JumpApexThreshold)
            {
                _ctx.SetGravityScale(_ctx.JumpApexGravityMult);
            }
            // Note: If strictly rising > threshold, gravity stays at UpwardGravityMult (set in Enter)
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
        }

        public override void ExitState()
        {
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.GetVelocity().y < 0)
            {
                SwitchState(_factory.FallState);
            }
            else if (_ctx.IsTouchingWall && _ctx.MoveInput.x == _ctx.FacingDirection && _ctx.GetVelocity().y < 0)
            {
                SwitchState(_factory.WallState);
            }
            else if (_ctx.IsDashing)
            {
                SwitchState(_factory.DashState);
            }
            // else if (_ctx.IsGrounded) // Should typically happen via Fall, but just in case
            // {
            //     SwitchState(_factory.GroundedState);
            // }
        }

        public override void InitializeSubState() { }

        private void HandleJump()
        {
            // Reset velocity Check
            _ctx.SetVelocity(new Vector2(_ctx.GetVelocity().x, 0));
            _ctx.AddForce(Vector2.up * _ctx.JumpForce, ForceMode2D.Impulse);
            
            _ctx.ApplySquashAndStretch();
            _ctx.ConsumeJumpInput();
        }
    }
}
