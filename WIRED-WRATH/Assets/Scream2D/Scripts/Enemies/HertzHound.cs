using UnityEngine;
using Scream2D.Controllers;

namespace Scream2D.Enemies
{
    public class HertzHound : EnemyBase
    {
        [Header("Jammer Settings")]
        [SerializeField] private float jamRadius = 6f;
        [SerializeField] private Color jamColor = new Color(0.5f, 0f, 0.5f, 0.3f); // Purple

        private PlayerController _targetPlayer;
        private bool _isJamming = false;

        protected override void Start()
        {
            base.Start();
            _targetPlayer = FindFirstObjectByType<PlayerController>();
        }

        protected override void PerformBehavior()
        {
            if (_targetPlayer == null) return;

            float dist = Vector2.Distance(transform.position, _targetPlayer.transform.position);
            
            // Chase logic
            if (dist < jamRadius * 2f && dist > jamRadius * 0.5f)
            {
                Vector2 dir = (_targetPlayer.transform.position - transform.position).normalized;
                transform.Translate(dir * moveSpeed * Time.deltaTime);
            }

            if (dist <= jamRadius)
            {
                if (!_isJamming)
                {
                    _isJamming = true;
                    ApplyJam(true);
                }
            }
            else
            {
                if (_isJamming)
                {
                    _isJamming = false;
                    ApplyJam(false);
                }
            }
            
            // Visuals: Draw line to player if jamming
            if (_isJamming)
            {
                Debug.DrawLine(transform.position, _targetPlayer.transform.position, jamColor);
            }
        }

        private void ApplyJam(bool state)
        {
            if (_targetPlayer == null) return;
            
            Debug.Log($"Hertz-Hound Jamming: {state}");
            _targetPlayer.SetJumpLock(state);
            _targetPlayer.SetDashLock(state);
            
            // Visual feedback on player
            if (state) 
                _targetPlayer.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.0f, 0.8f); // Purple tint
            else
                _targetPlayer.GetComponent<SpriteRenderer>().color = Color.white;
        }

        protected override void HandleStun()
        {
            // If stunned, disable jam
            if (_isJamming)
            {
                _isJamming = false;
                ApplyJam(false);
            }
            base.HandleStun();
        }

        public override void OnScreamHit(Vector2 sourcePosition, float power)
        {
            // Instantly Die as per design
            Debug.Log("Hertz-Hound Overloaded!");
            if (_isJamming) ApplyJam(false); // Release player before dying
            Die(); 
        }

        protected override void Die()
        {
            // Ensure we verify release one last time
            if (_isJamming) ApplyJam(false);
            
            // Improved death FX (Explosion)
            // ParticleSystem logic would go here
            
            base.Die();
        }

        private void OnDestroy()
        {
            // Failsafe
            if (_isJamming) ApplyJam(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = jamColor;
            Gizmos.DrawWireSphere(transform.position, jamRadius);
        }
    }
}
