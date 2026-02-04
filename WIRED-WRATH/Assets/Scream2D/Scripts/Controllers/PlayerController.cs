using UnityEngine;
using Zenject;
using DG.Tweening;
using Scream2D.Systems;
using Scream2D.Controllers.StateMachine;
using Scream2D.Enemies;
using Scream2D.Projectiles;
using Scream2D.Interfaces;

namespace Scream2D.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        #region References
        [Header("Components")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        
        [SerializeField] private ParticleFactory _particleFactory;
        [SerializeField] private GhostTrail _ghostTrail;
        
        private ScreamMeter _screamMeter;
        #endregion

        #region Configuration
        [Header("Movement Stats")]
        public float MoveSpeed = 10f;
        public float Acceleration = 120f;
        public float Deceleration = 120f;
        public float AirControl = 1f;

        [Header("Jump Stats")]
        public float JumpForce = 14f;
        public float VariableJumpHeightMultiplier = 0.5f;
        public float CoyoteTime = 0.15f;
        public float JumpBufferTime = 0.2f;
        public float JumpApexThreshold = 2f; 
        public float JumpApexGravityMult = 0.5f;
        public float FallGravityMult = 4f; 
        public float UpwardGravityMult = 0.8f;
        public int MaxAirJumps = 0;

        [Header("Wall Stats")]
        public float WallSlideSpeed = 2f;
        public float WallClimbSpeed = 4f;
        public Vector2 WallJumpForce = new Vector2(12f, 14f);
        public float WallCheckDistance = 0.4f;
        public LayerMask WallLayer;
        public float MaxStamina = 100f;
        
        public float CornerCorrectionDistance = 0.1f;
        
        [Header("Dash Stats")]
        public float DashSpeed = 30f;
        public float DashDuration = 0.15f;
        public float DashCooldown = 0.2f;
        public float WaveDashWindow = 0.15f;
        public float WaveDashVelocityMult = 1.5f;

        [Header("Game Feel")]
        public float SquashAmount = 0.8f;
        public float StretchAmount = 1.2f;
        public float TweenDuration = 0.15f;
        #endregion

        #region State Machine
        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerBaseState CurrentState => StateMachine?.CurrentState;
        #endregion

        #region Runtime Variables
        public Vector2 MoveInput { get; private set; }
        public bool IsGrounded { get; set; }
        public bool IsTouchingWall { get; private set; }
        public bool IsJumpPressed { get; set; }
        public bool IsJumpHeld { get; private set; }
        public bool IsGrabHeld { get; private set; }
        public float LastGroundedTime { get; set; }
        public float LastJumpPressedTime { get; set; }
        public float LastDashTime { get; set; }
        public float DashEndTime { get; set; } // Correct property
        public int FacingDirection { get; private set; } = 1;
        public bool IsDashing { get; set; }
        public float CurrentStamina { get; set; }
        public int CurrentAirJumps { get; set; }
        public bool CanDash { get; set; } = true;
        
        private int _groundLayer;
        #endregion

        [Inject]
        public void Construct(ScreamMeter screamMeter)
        {
            _screamMeter = screamMeter;
        }

        private void Awake()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_animator == null) _animator = GetComponent<Animator>();

            StateMachine = new PlayerStateMachine();
            _groundLayer = LayerMask.GetMask("Ground");

            StateMachine.GroundedState = new PlayerGroundedState(this, StateMachine);
            StateMachine.JumpState = new PlayerJumpState(this, StateMachine);
            StateMachine.FallState = new PlayerFallState(this, StateMachine);
            StateMachine.WallState = new PlayerWallState(this, StateMachine);
            StateMachine.DashState = new PlayerDashState(this, StateMachine);
            StateMachine.ClimbState = new PlayerClimbState(this, StateMachine);
        }

        private void Start()
        {
            StateMachine.Initialize(StateMachine.GroundedState);
        }

        private void Update()
        {
            HandleInput();
            HandleAbilities();
            
            IsGrounded = Physics2D.OverlapCircle(transform.position + Vector3.up * 0.1f, 0.2f, _groundLayer);
            IsTouchingWall = Physics2D.Raycast(transform.position, Vector2.right * FacingDirection, WallCheckDistance, WallLayer);

            if (IsGrounded)
            {
                LastGroundedTime = Time.time;
                if (IsGroundPounding) LandGroundPound();
            }

            StateMachine.Update();
        }

        private void FixedUpdate()
        {
            StateMachine.FixedUpdate();
            CheckCornerCorrection();
        }

        public bool IsInputBlocked { get; private set; }

        public void SetInputBlocked(bool blocked)
        {
            IsInputBlocked = blocked;
            if (blocked)
            {
                MoveInput = Vector2.zero;
                IsJumpPressed = false;
                IsJumpHeld = false;
            }
        }

        private void HandleInput()
        {
            if (IsInputBlocked) return;

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(x, y);

            if (x != 0)
            {
                FacingDirection = x > 0 ? 1 : -1;
            }

            if (Input.GetButtonDown("Jump"))
            {
                IsJumpPressed = true;
                LastJumpPressedTime = Time.time;
            }
            
            IsJumpHeld = Input.GetButton("Jump");
            IsGrabHeld = Input.GetButton("Fire2") || Input.GetKey(KeyCode.LeftShift);
            
            // Check Locks
            if (IsJumpLocked) 
            {
                IsJumpPressed = false;
                IsJumpHeld = false;
            }

            bool dashInput = (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.K) || Input.GetButtonDown("Fire3"));
            if (dashInput && Time.time - LastDashTime > DashCooldown && CanDash && !IsDashLocked)
            {
                if (!IsDashing) 
                {
                    IsDashing = true; 
                    // CanDash will be set to false in DashState.EnterState
                }
            }
        }

        private int _jumpLockCount = 0;
        private int _dashLockCount = 0;
        public bool IsJumpLocked => _jumpLockCount > 0;
        public bool IsDashLocked => _dashLockCount > 0;

        public void SetJumpLock(bool state) 
        {
            _jumpLockCount = Mathf.Max(0, _jumpLockCount + (state ? 1 : -1));
        }
        public void SetDashLock(bool state)
        {
            _dashLockCount = Mathf.Max(0, _dashLockCount + (state ? 1 : -1));
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * FacingDirection * WallCheckDistance);
        }
        
        private void CheckCornerCorrection()
        {
             if (_rb.linearVelocity.y > 0)
             {
                 // Logic would go here
             }
        }

        public void SetVelocity(Vector2 velocity)
        {
            _rb.linearVelocity = velocity;
        }

        public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
        {
            _rb.AddForce(force, mode);
        }
        
        public Vector2 GetVelocity()
        {
            return _rb.linearVelocity;
        }

        public void FlipSprite(bool faceRight)
        {
            _spriteRenderer.flipX = !faceRight;
        }
        
        public void ApplySquashAndStretch()
        {
            transform.DOScale(new Vector3(SquashAmount, StretchAmount, 1f), TweenDuration)
                .OnComplete(() => transform.DOScale(Vector3.one, TweenDuration));
        }
        
        public void ConsumeJumpInput()
        {
            IsJumpPressed = false;
        }
        
        public void ApplyImpact()
        {
             Camera.main.transform.DOShakePosition(0.3f, 0.2f);
            _spriteRenderer.DOColor(Color.red, 0.1f).OnComplete(() => _spriteRenderer.DOColor(Color.white, 0.2f));
        }

        public void SetGravityScale(float scale)
        {
            _rb.gravityScale = scale;
        }

        public void FreezeFrame(float duration)
        {
            Time.timeScale = 0f;
            DOVirtual.DelayedCall(duration, () => Time.timeScale = 1f).SetUpdate(true);
        }
        
        public void PlayJumpDust()
        {
            if (_particleFactory) _particleFactory.PlayJumpDust(transform.position + Vector3.down * 0.5f);
        }
        
        public void StartGhostTrail()
        {
            if (_ghostTrail) _ghostTrail.StartEmitting();
        }
        
        public void StopGhostTrail()
        {
            if (_ghostTrail) _ghostTrail.StopEmitting();
        }
        
        public float ScreamRadius = 5f;
        public float ScreamCooldown = 5f;
        public float LastScreamTime { get; private set; } = -10f; 

        public float GlitchStepDistance = 3f;
        public float GlitchCooldown = 2f;
        public float LastGlitchTime { get; private set; } = -10f;
        public bool IsInvulnerable { get; private set; }

        public bool IsGroundPounding { get; private set; }
        public float GroundPoundForce = 25f;

        private void HandleAbilities()
        {
             // Ability 1: Scream Pulse (Q)
             if (Input.GetKeyDown(KeyCode.Q) && Time.time - LastScreamTime >= ScreamCooldown)
             {
                 PerformScreamPulse();
             }

             // Ability 2: Glitch Step (Left Shift)
             if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time - LastGlitchTime >= GlitchCooldown && !IsDashLocked)
             {
                 PerformGlitchStep();
             }

             // Ability 3: Memory Anchor (Down + Jump in Air)
             if (!IsGrounded && Input.GetAxisRaw("Vertical") < -0.5f && Input.GetButtonDown("Jump") && !IsGroundPounding)
             {
                 StartGroundPound();
             }

             // Ability 4: Logic Virus (F)
             if (Input.GetKeyDown(KeyCode.F))
             {
                 FireLogicVirus();
             }
        }

        [Header("Projectile Refs")]
        public GameObject LogicVirusPrefab;

        private void FireLogicVirus()
        {
             if (LogicVirusPrefab == null) 
             {
                 Debug.LogWarning("[Scream2D] LogicVirusPrefab is NULL on PlayerController! Please assign it in the Inspector or run Auto-Setup.");
                 return;
             }
             
             GameObject viral = Instantiate(LogicVirusPrefab, transform.position, Quaternion.identity);
             var projectile = viral.GetComponent<LogicVirusProjectile>();
             if (projectile != null)
             {
                 projectile.Direction = new Vector2(FacingDirection, 0);
             }
             
             // Recoil
             if (!IsGrounded) SetVelocity(GetVelocity() + new Vector2(-FacingDirection * 2f, 0));
        }

        private void StartGroundPound()
        {
            IsGroundPounding = true;
            IsInvulnerable = true; // Temporary frame protection
            SetVelocity(new Vector2(0, -GroundPoundForce));
            StartCoroutine(FlashColor(Color.grey, 0.5f)); // Heavy color
        }

        public void LandGroundPound()
        {
            if (!IsGroundPounding) return;

            IsGroundPounding = false;
            IsInvulnerable = false;
            
            // Impact Juice
            Camera.main.transform.DOShakePosition(0.4f, 0.8f);
            if (_particleFactory) _particleFactory.PlayGroundPoundImpact(transform.position);
            
            // Area Damage
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3f);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<EnemyBase>();
                if (enemy) enemy.OnGroundPoundHit();
            }
            
            Debug.Log("MEMORY ANCHOR LANDED!");
        }

        private System.Collections.IEnumerator FlashColor(Color color, float duration)
        {
            if (_spriteRenderer == null) yield break;
            Color original = _spriteRenderer.color;
            _spriteRenderer.color = color;
            yield return new WaitForSeconds(duration);
            _spriteRenderer.color = original;
        }

        private void PerformScreamPulse()
        {
            LastScreamTime = Time.time;
            Camera.main.transform.DOShakePosition(0.5f, 0.5f);
            if (_particleFactory) _particleFactory.PlayScreamPulse(transform.position);
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ScreamRadius);
            foreach (var hit in hits)
            {
                var listener = hit.GetComponent<IScreamListener>();
                listener?.OnScreamHit(transform.position, 10f);
            }
        }

        private void PerformGlitchStep()
        {
            LastGlitchTime = Time.time;
            IsInvulnerable = true;
            if (_particleFactory) _particleFactory.PlayGlitchEffect(transform.position);
            
            // Calculate target position
            Vector2 dir = new Vector2(FacingDirection, 0);
            Vector2 targetPos = (Vector2)transform.position + dir * GlitchStepDistance;
            
            // Wall check
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, GlitchStepDistance, WallLayer);
            if (hit.collider != null)
            {
                targetPos = hit.point - dir * 0.5f; // Stop before wall
            }

            // Teleport
            transform.position = targetPos;
            
            // Visuals
            _spriteRenderer.color = new Color(0, 1, 1, 0.5f); // Cyan transparent
            DOVirtual.DelayedCall(0.2f, () => 
            {
                IsInvulnerable = false;
                _spriteRenderer.color = Color.white;
            });
            
            Debug.Log("GLITCH STEP!");
        }

        public void PlayAnimation(string animationName)
        {
            if (_animator != null)
            {
                _animator.Play(animationName);
            }
        }
    }
}
