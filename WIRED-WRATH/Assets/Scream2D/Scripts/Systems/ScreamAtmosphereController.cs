using UnityEngine;
using Zenject;

namespace Scream2D.Systems
{
    public class ScreamAtmosphereController : MonoBehaviour
    {
        [SerializeField] private Material distortionMaterial;
        private ScreamMeter _screamMeter;

        [Inject]
        public void Construct(ScreamMeter screamMeter)
        {
            _screamMeter = screamMeter;
        }

        private void Update()
        {
            if (distortionMaterial != null && _screamMeter != null)
            {
                distortionMaterial.SetFloat("_Intensity", _screamMeter.normalizedScream);
                
                // Dynamic FOV or Vignette could also be handled here
                Camera.main.orthographicSize = 3.5f - (_screamMeter.normalizedScream * 0.5f);
            }
        }
    }
}
