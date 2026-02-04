using UnityEngine;
using Scream2D.Enemies;

namespace Scream2D.Enemies
{
    public class BitFly : EnemyBase
    {
        [Header("Swarm Settings")]
        [SerializeField] private float chainRadius = 2.5f;
        [SerializeField] private float wobbleSpeed = 3f;
        [SerializeField] private float wobbleAmount = 0.5f;
        
        private Vector3 _startPos;
        private bool _isInfected = false;

        protected override void Start()
        {
            base.Start();
            _startPos = transform.position;
            // Make them small
            transform.localScale = Vector3.one * 0.5f;
        }

        protected override void PerformBehavior()
        {
            // Idle wobble
            transform.position = _startPos + (Vector3)UnityEngine.Random.insideUnitCircle * Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        }

        public override void OnScreamHit(Vector2 sourcePosition, float power)
        {
            // Bit-flies are so light, they don't just stun, they scatter
            base.OnScreamHit(sourcePosition, power);
            _startPos = transform.position; // New anchor after scatter
        }

        public override void OnVirusHit()
        {
            if (_isInfected) return;
            _isInfected = true;

            // Chain Reaction!
            Debug.Log("Bit-Fly Infected! Exploding...");
            
            var factory = FindFirstObjectByType<Systems.ParticleFactory>();
            if (factory) factory.PlayVirusExplosion(transform.position);

            // Find neighbors
            Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, chainRadius);
            foreach (var n in neighbors)
            {
                if (n.gameObject == gameObject) continue; // Skip self

                if (n.TryGetComponent<BitFly>(out var otherFly))
                {
                    // Delay slightly for visual flair
                    // Or instant for efficiency. Let's do instant but maybe logic-gated.
                     if (otherFly.CanTakeDamage()) // Basic check to avoid infinite loop on dead ones if destroy is delayed
                     {
                         // We can recursively call OnVirusHit, but risk stack overflow if huge.
                         // Better: Set them to die next frame or invoke.
                         // Simple way: Just call it. Game shouldn't have 1000s.
                         otherFly.TakeDamage(100); // Kill it
                         otherFly.OnVirusHit(); // Propagate
                     }
                }
            }
            
            Die();
        }

        public override bool CanTakeDamage()
        {
            return currentHealth > 0;
        }
    }
}
