using UnityEngine;
using Scream2D.Controllers;
using Scream2D.Enemies;
using DG.Tweening;

namespace Scream2D.Enemies
{
    public class OculusNull : EnemyBase
    {
        [Header("Patrol Settings")]
        [SerializeField] private float patrolRadius = 3f;
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float exposedDuration = 4f;

        [Header("Combat")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private float sightRange = 10f;
        
        private Vector3 startPos;
        private bool isExposed = false;
        private float exposedTimer = 0f;
        private float fireTimer = 0f;
        private PlayerController _targetPlayer;

        protected override void Start()
        {
            base.Start();
            startPos = transform.position;
            _targetPlayer = FindFirstObjectByType<PlayerController>();
        }

        protected override void PerformBehavior()
        {
            // Simple Floating Patrol (Sine wave)
            float x = startPos.x + Mathf.Sin(Time.time * patrolSpeed) * patrolRadius;
            float y = startPos.y + Mathf.Cos(Time.time * patrolSpeed * 0.5f) * 0.5f;
            transform.position = new Vector3(x, y, 0);

            if (_targetPlayer != null && !isStunned)
            {
                HandleCombat();
            }

            if (isExposed)
            {
                exposedTimer -= Time.deltaTime;
                if (exposedTimer <= 0)
                {
                    CloseEye();
                }
            }
        }

        private void HandleCombat()
        {
            float dist = Vector2.Distance(transform.position, _targetPlayer.transform.position);
            if (dist < sightRange)
            {
                // Line of sight check
                RaycastHit2D hit = Physics2D.Linecast(transform.position, _targetPlayer.transform.position, LayerMask.GetMask("Ground", "Wall"));
                if (hit.collider == null)
                {
                    fireTimer -= Time.deltaTime;
                    if (fireTimer <= 0)
                    {
                        Shoot();
                        fireTimer = fireRate;
                    }
                }
            }
        }

        private void Shoot()
        {
            if (projectilePrefab == null) return;
            
            Debug.Log("Oculus-Null Firing!");
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            var rbProj = proj.GetComponent<Projectiles.EnemyProjectile>();
            if (rbProj != null)
            {
                rbProj.Direction = (_targetPlayer.transform.position - transform.position).normalized;
            }
        }

        public override void OnScreamHit(Vector2 sourcePosition, float power)
        {
            base.OnScreamHit(sourcePosition, power); // Apply Stun
            OpenEye();
        }

        private void OpenEye()
        {
            if (isExposed) return;
            
            isExposed = true;
            exposedTimer = exposedDuration;
            
            // Visual feedback: Eye opens (Scale up or change color)
            transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack);
            if (spriteRenderer) spriteRenderer.color = Color.yellow; // Yellow = Vulnerable
            
            Debug.Log("Oculus-Null is EXPOSED!");
        }

        private void CloseEye()
        {
            isExposed = false;
            transform.DOScale(1f, 0.3f);
            if (spriteRenderer) spriteRenderer.color = Color.white;
            Debug.Log("Oculus-Null closed its eye.");
        }

        public override bool CanTakeDamage()
        {
            // Only vulnerable when Exposed
            return isExposed;
        }

        protected override void Die()
        {
            // Death Logic
            Debug.Log("Oculus-Null Destroyed!");
            transform.DOScale(0f, 0.2f).OnComplete(() => base.Die());
        }
    }
}
