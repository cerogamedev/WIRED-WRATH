using UnityEngine;
using DG.Tweening;

namespace Scream2D.Systems
{
    public class GhostTrail : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _targetSprite;
        [SerializeField] private float _spawnRate = 0.05f;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Color _trailColor = new Color(1, 1, 1, 0.5f);

        private float _lastSpawnTime;
        private bool _isEmitting;

        public void StartEmitting()
        {
            _isEmitting = true;
        }

        public void StopEmitting()
        {
            _isEmitting = false;
        }

        private void Update()
        {
            if (_isEmitting && Time.time - _lastSpawnTime > _spawnRate)
            {
                SpawnGhost();
                _lastSpawnTime = Time.time;
            }
        }

        private void SpawnGhost()
        {
            if (_targetSprite == null) return;

            GameObject ghost = new GameObject("Ghost");
            ghost.transform.position = _targetSprite.transform.position;
            ghost.transform.rotation = _targetSprite.transform.rotation;
            ghost.transform.localScale = _targetSprite.transform.localScale;

            SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = _targetSprite.sprite;
            sr.color = _trailColor;
            sr.sortingLayerID = _targetSprite.sortingLayerID;
            sr.sortingOrder = _targetSprite.sortingOrder - 1;

            sr.DOFade(0, _fadeDuration).OnComplete(() => Destroy(ghost));
        }
    }
}
