using UnityEngine;
using Zenject;
using Scream2D.Controllers;

namespace Scream2D.Systems
{
    public class BioHazard : MonoBehaviour
    {
        [SerializeField] private float screamDamage = 20f;
        
        private ScreamMeter _screamMeter;

        [Inject]
        public void Construct(ScreamMeter screamMeter)
        {
            _screamMeter = screamMeter;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _screamMeter?.AddScream(screamDamage);
                
                if (collision.gameObject.TryGetComponent<PlayerController>(out var player))
                {
                    player.ApplyImpact();
                }
                Debug.Log("BioHazard touched! Scream level increased.");
            }
        }
    }
}
