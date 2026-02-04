using UnityEngine;
using Scream2D.Enemies;

namespace Scream2D.Projectiles
{
    public class LogicVirusProjectile : MonoBehaviour
    {
        public float Speed = 15f;
        public float LifeTime = 3f;
        public Vector2 Direction { get; set; } = Vector2.right;

        private void Start()
        {
            Destroy(gameObject, LifeTime);
        }

        private void Update()
        {
            transform.Translate(Direction * Speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.OnVirusHit();
                CmdExplode();
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                CmdExplode();
            }
        }

        private void CmdExplode()
        {
            var factory = FindFirstObjectByType<Systems.ParticleFactory>();
            if (factory) factory.PlayVirusExplosion(transform.position);
            Destroy(gameObject);
        }
    }
}
