using UnityEngine;
using Scream2D.Enemies;

namespace Scream2D.Enemies
{
    public class ServerCrab : EnemyBase
    {
        [SerializeField] private float aggroRange = 8f;
        [SerializeField] private float aggroSpeedMult = 2f;
        
        private int _moveDir = 1;
        private PlayerController _player;

        protected override void Start()
        {
            base.Start();
            _player = FindFirstObjectByType<PlayerController>();
        }

        protected override void PerformBehavior()
        {
            float currentSpeed = moveSpeed;
            
            // Aggro check
            if (_player != null)
            {
                float dist = Vector2.Distance(transform.position, _player.transform.position);
                if (dist < aggroRange)
                {
                    currentSpeed *= aggroSpeedMult;
                    _moveDir = (_player.transform.position.x > transform.position.x) ? 1 : -1;
                }
            }

            // Wall/Edge check (Simple version using Raycast)
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * _moveDir, 1f, LayerMask.GetMask("Ground", "Wall"));
            if (hit.collider != null)
            {
                _moveDir *= -1;
            }

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(_moveDir * currentSpeed, rb.linearVelocity.y);
            }
            
            // Visual orientation
            if (spriteRenderer != null) spriteRenderer.flipX = (_moveDir < 0);
        }

        public override bool CanTakeDamage()
        {
            // Immune if rolled up
            return !isRolledUp; 
        }

        public override void OnGroundPoundHit()
        {
            if (isRolledUp)
            {
                Debug.Log("Server-Crab shell CRUSHED by Memory Anchor!");
                isRolledUp = false;
                // Visuals: Shield break effect
                if (spriteRenderer) spriteRenderer.color = Color.red; 
                TakeDamage(100f); // Instant Kill logic for now
            }
            else
            {
                base.OnGroundPoundHit();
            }
        }
    }
}
