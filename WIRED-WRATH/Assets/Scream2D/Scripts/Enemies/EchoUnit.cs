using UnityEngine;
using System.Collections.Generic;
using Scream2D.Controllers;

namespace Scream2D.Enemies
{
    public class EchoUnit : EnemyBase
    {
        [Header("Echo Settings")]
        [SerializeField] private float delaySeconds = 1.0f;
        [SerializeField] private float recordInterval = 0.05f;
        [SerializeField] private float damage = 15f;

        private PlayerController _targetPlayer;
        private Queue<Vector3> _positionHistory = new Queue<Vector3>();
        private float _recordTimer = 0f;
        private float _maxStoredPositions;

        protected override void Start()
        {
            base.Start();
            _targetPlayer = FindFirstObjectByType<PlayerController>();
            _maxStoredPositions = delaySeconds / recordInterval;
            
            // Make semi-transparent black/dark
            if (spriteRenderer) spriteRenderer.color = new Color(0, 0, 0, 0.5f);
            
            // No physics collision with walls, just a trigger
            if (GetComponent<Collider2D>()) GetComponent<Collider2D>().isTrigger = true;
            if (rb) rb.gravityScale = 0;
        }

        protected override void PerformBehavior()
        {
            if (_targetPlayer == null) return;

            // Record Player Position
            _recordTimer += Time.deltaTime;
            if (_recordTimer >= recordInterval)
            {
                _recordTimer = 0;
                _positionHistory.Enqueue(_targetPlayer.transform.position);
                
                // Trim history
                if (_positionHistory.Count > _maxStoredPositions)
                {
                    Vector3 targetPos = _positionHistory.Dequeue();
                    transform.position = targetPos;
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                if (player.IsInvulnerable)
                {
                    // Player Glitch-Stepped THROUGH us!
                    Debug.Log("Echo-Unit Glitched Out!");
                    Die();
                }
                else
                {
                    // Player Hit!
                    var meter = FindFirstObjectByType<Systems.ScreamMeter>();
                    if (meter != null)
                    {
                        meter.AddScream(damage * Time.deltaTime); // Continuous damage
                    }
                    Debug.Log("Echo-Unit draining Player's sanity!");
                }
            }
        }

        public override void OnScreamHit(Vector2 sourcePosition, float power)
        {
            // Echo Units are just data ghosts, maybe they flicker but don't care about scream?
            // Or maybe Scream dispels them too?
            // Design doc doesn't specify, but let's make them stun-able like base enemies.
            base.OnScreamHit(sourcePosition, power);
        }
    }
}
