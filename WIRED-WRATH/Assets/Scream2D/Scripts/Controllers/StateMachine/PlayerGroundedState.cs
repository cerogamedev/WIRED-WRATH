using UnityEngine;

namespace Scream2D.Controllers.StateMachine
{
    public class PlayerGroundedState : PlayerBaseState
    {
        public PlayerGroundedState(PlayerController currentContext, PlayerStateMachine playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.IsGrounded = true; 
            _ctx.CurrentAirJumps = 0; // Reset Double Jump
            _ctx.CanDash = true;      // Reset Dash Charge
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            
            // Movement Logic
            float targetSpeed = _ctx.MoveInput.x * _ctx.MoveSpeed;
            float speedDif = targetSpeed - _ctx.GetVelocity().x;
            
            // Accel/Decel
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _ctx.Acceleration : _ctx.Deceleration;
            float movement = speedDif * accelRate * Time.deltaTime; // Force-based or velocity-based? 
            // Let's use direct velocity modification for snappiness as per "Platformer" feel, or AddForce for physics.
            // The user want "Professional". Rigidboby2D.velocity is often preferred for tight controls over AddForce.
            // But we used FixedUpdate in the old script. Let's use velocity setting in FixedUpdate for consistency or force.
            // Let's stick to setting velocity for x, preserving y.
        }

        public override void FixedUpdateState()
        {
             // Smooth Movement (Velocity based for responsiveness)
             float targetSpeed = _ctx.MoveInput.x * _ctx.MoveSpeed;
             // Ideally we leverage dotween or smoothdamp, but direct velocity is standard for 2D platformers.
             Vector2 currentVel = _ctx.GetVelocity();
             float newX = Mathf.MoveTowards(currentVel.x, targetSpeed, (_ctx.MoveInput.x != 0 ? _ctx.Acceleration : _ctx.Deceleration) * Time.fixedDeltaTime);
             
             _ctx.SetVelocity(new Vector2(newX, currentVel.y));

             if (Mathf.Abs(newX) > 0.1f)
             {
                 _ctx.FlipSprite(newX > 0);
             }
        }

        public override void ExitState()
        {
            _ctx.LastGroundedTime = Time.time;
        }

        public override void CheckSwitchStates()
        {
            // Jump Buffer Check
            if (_ctx.IsJumpPressed && (Time.time - _ctx.LastJumpPressedTime) < _ctx.JumpBufferTime)
            {
                 if (Time.time - _ctx.DashEndTime < _ctx.WaveDashWindow)
                 {
                     // Wave Dash / Hyper Jump
                     PerformWaveDash();
                 }
                 else
                 {
                     SwitchState(_factory.JumpState);
                 }
            }
            else if (_ctx.IsDashing)
            {
                SwitchState(_factory.DashState);
            }
            else if (!_ctx.IsGrounded)
            {
                SwitchState(_factory.FallState);
            }
        }

        public override void InitializeSubState() { }
        
        private void PerformWaveDash()
        {
            // Boost horizontal speed and jump low
            Vector2 waveVel = new Vector2(_ctx.DashSpeed * _ctx.FacingDirection * _ctx.WaveDashVelocityMult, _ctx.JumpForce * 0.7f); // Lower jump for wave dash often better
            _ctx.SetVelocity(waveVel);
            
            _ctx.PlayJumpDust(); // Juice
            _ctx.FreezeFrame(0.02f); // Tiny hitstop for tech execution
            _ctx.ConsumeJumpInput();
            
            // Switch to Fall or Jump? 
            // Jump state might override horizontal velocity in Update...
            // Let's switch to Jump State but we need to ensure it doesn't clamp velocity.
            // Or just FallState.
            SwitchState(_factory.FallState);
        }
    }
}
