using UnityEngine;
using Zenject;
using Scream2D.Controllers;

namespace Scream2D.Systems
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        
        [Header("Settings")]
        public float SmoothTime = 0.15f;
        public Vector3 Offset = new Vector3(0, 1f, -10f);
        public float DefaultOrthographicSize = 3.5f;
        
        private Vector3 _currentVelocity = Vector3.zero;
        private Camera _camera;

        [Inject]
        public void Construct(PlayerController player)
        {
            if (player != null) _target = player.transform;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera != null) _camera.orthographicSize = DefaultOrthographicSize;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPosition = _target.position + Offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, SmoothTime);
        }

        public void SetZoom(float size)
        {
            if (_camera != null) _camera.orthographicSize = size;
        }
    }
}
