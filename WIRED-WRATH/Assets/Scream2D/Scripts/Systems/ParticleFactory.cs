using UnityEngine;

namespace Scream2D.Systems
{
    public class ParticleFactory : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _jumpDust;
        [SerializeField] private ParticleSystem _wallDust;
        [SerializeField] private ParticleSystem _dashLines;

        public void PlayJumpDust(Vector3 position)
        {
            if (_jumpDust) Instantiate(_jumpDust, position, Quaternion.identity);
        }

        public void PlayWallDust(Vector3 position, int direction)
        {
             // Directional logic for wall dust
             if (_wallDust) Instantiate(_wallDust, position, Quaternion.identity);
        }

        public void PlayDashLines(Vector3 position)
        {
             if (_dashLines) Instantiate(_dashLines, position, Quaternion.identity);
        }

        [Header("Abilities")]
        [SerializeField] private ParticleSystem _screamPulse;
        [SerializeField] private ParticleSystem _glitchEffect;
        [SerializeField] private ParticleSystem _groundPoundImpact;
        [SerializeField] private ParticleSystem _virusExplosion;
        
        public void PlayScreamPulse(Vector3 position)
        {
            if (_screamPulse) Instantiate(_screamPulse, position, Quaternion.identity);
        }

        public void PlayGlitchEffect(Vector3 position)
        {
            if (_glitchEffect) Instantiate(_glitchEffect, position, Quaternion.identity);
        }

        public void PlayGroundPoundImpact(Vector3 position)
        {
            if (_groundPoundImpact) Instantiate(_groundPoundImpact, position, Quaternion.identity);
        }

        public void PlayVirusExplosion(Vector3 position)
        {
            if (_virusExplosion) Instantiate(_virusExplosion, position, Quaternion.identity);
        }
    }
}
