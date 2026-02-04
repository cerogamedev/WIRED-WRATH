using UnityEngine;
using Scream2D.Controllers;

namespace Scream2D.Projectiles
{
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifetime = 5f;

        public Vector2 Direction { get; set; }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.Translate(Direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                // Hasar verme mantığı: PlayerController içindeki AddScream metoduna bağlanır
                var screamMeter = FindFirstObjectByType<Systems.ScreamMeter>();
                if (screamMeter != null)
                {
                    screamMeter.AddScream(damage);
                }
                
                // Oyuncuya hafif bir itme (knockback) uygula
                player.ApplyImpact();
                
                Destroy(gameObject);
            }
            else if (((1 << other.gameObject.layer) & LayerMask.GetMask("Ground", "Wall")) != 0)
            {
                // Duvara çarpınca yok ol
                Destroy(gameObject);
            }
        }
    }
}
