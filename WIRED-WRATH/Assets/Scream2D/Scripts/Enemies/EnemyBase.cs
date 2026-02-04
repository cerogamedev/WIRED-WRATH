using UnityEngine;
using Scream2D.Interfaces;

namespace Scream2D.Enemies
{
    public abstract class EnemyBase : MonoBehaviour, IScreamListener
    {
        [Header("Enemy Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float stunDuration = 3f;

        protected float currentHealth;
        protected bool isStunned = false;
        protected float stunTimer = 0f;
        
        protected Rigidbody2D rb;
        protected SpriteRenderer spriteRenderer;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Update()
        {
            if (isStunned)
            {
                HandleStun();
                return;
            }

            PerformBehavior();
        }

        protected virtual void HandleStun()
        {
            stunTimer -= Time.deltaTime;
            
            // Visual flicker
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 10, 1));
            }

            if (stunTimer <= 0)
            {
                isStunned = false;
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
                OnStunEnd();
            }
        }

        // Abstract method for specific enemy logic (Patrol, Chase, etc.)
        protected abstract void PerformBehavior();

        // Check if enemy CAN be damaged currently
        public virtual bool CanTakeDamage()
        {
            return true; // Default yes, overridden by specific enemies like Oculus-Null
        }

        public virtual void TakeDamage(float amount)
        {
            if (!CanTakeDamage())
            {
                // Play metallic "Clank" sound or deflection effect
                Debug.Log($"{name} deflected the attack!");
                return;
            }

            currentHealth -= amount;
            Debug.Log($"{name} took {amount} damage. HP: {currentHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Hit effect
                if (spriteRenderer != null) 
                    StartCoroutine(FlashColor(Color.white, 0.1f));
            }
        }

        public virtual void OnScreamHit(Vector2 sourcePosition, float power)
        {
            if (isStunned) return;

            Debug.Log($"{name} hit by Scream Pulse!");
            isStunned = true;
            stunTimer = stunDuration;
            
            // Pushback
            if (rb != null)
            {
                Vector2 dir = (transform.position - (Vector3)sourcePosition).normalized;
                rb.AddForce(dir * power * 10f, ForceMode2D.Impulse);
            }
        }

        public virtual void OnGroundPoundHit()
        {
            // Default behavior: heavy damage or knockup?
            // For now, take moderate damage
            TakeDamage(30f);
        }

        public virtual void OnVirusHit()
        {
            // Default: Small damage + DoT Visuals
            TakeDamage(10f);
            if (spriteRenderer) StartCoroutine(FlashColor(Color.green, 0.5f));
        }

        protected virtual void OnStunEnd()
        {
            // Reset visuals, maybe agro
        }

        protected virtual void Die()
        {
            // Spawn death particles
            Destroy(gameObject);
        }

        private System.Collections.IEnumerator FlashColor(Color flashColor, float time)
        {
            if (spriteRenderer == null) yield break;
            Color original = spriteRenderer.color;
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(time);
            spriteRenderer.color = original;
        }
    }
}
