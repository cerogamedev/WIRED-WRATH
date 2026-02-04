using UnityEngine;
using Zenject;

namespace Scream2D.Systems
{
    public class ScreamMeter : MonoBehaviour
    {
        [SerializeField] private float maxScreams = 100f;
        [SerializeField] private float decayRate = 5f;
        [SerializeField] private float currentScream = 0f;
        [SerializeField] private bool oneHitKill = true;

        public float normalizedScream => currentScream / maxScreams;

        public event System.Action OnMaxScreamReached;

        public void AddScream(float amount)
        {
            if (amount <= 0) return;

            if (oneHitKill)
            {
                currentScream = maxScreams;
            }
            else
            {
                currentScream = Mathf.Clamp(currentScream + amount, 0, maxScreams);
            }
            
            if (currentScream >= maxScreams)
            {
                OnMaxScreamReached?.Invoke();
            }
        }

        private void Update()
        {
            if (currentScream > 0)
            {
                currentScream -= decayRate * Time.deltaTime;
            }
        }
    }
}
