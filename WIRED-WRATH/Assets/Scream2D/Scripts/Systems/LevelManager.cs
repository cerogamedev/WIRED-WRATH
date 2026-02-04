using UnityEngine;
using System.Collections.Generic;
using Zenject;
using Scream2D.Data;
using Scream2D.Level;
using UnityEngine.Rendering.Universal;
using Scream2D.Controllers;

namespace Scream2D.Systems
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private List<LevelData> _campaignLevels;
        [SerializeField] private Transform _levelContainer;
        [SerializeField] private Vector3 _startPosition = new Vector3(0, 0, 0);

        [Header("References")]
        [SerializeField] private Light2D _globalLight;

        private PlayerController _player;
        private ScreamMeter _screamMeter;
        private List<GameObject> _currentSegments = new List<GameObject>();
        private int _currentLevelIndex = 0;
        private DiContainer _container;

        [Inject]
        public void Construct([Inject(Optional = true)] PlayerController player, ScreamMeter screamMeter, DiContainer container)
        {
            _player = player;
            _screamMeter = screamMeter;
            _container = container;
        }

        private void Start()
        {
            if (_screamMeter != null)
            {
                _screamMeter.OnMaxScreamReached += RestartLevel;
            }

            if (_campaignLevels != null && _campaignLevels.Count > 0)
            {
                LoadLevel(_currentLevelIndex);
            }
        }

        private void OnDestroy()
        {
            if (_screamMeter != null)
            {
                _screamMeter.OnMaxScreamReached -= RestartLevel;
            }
        }

        public void LoadLevel(int index)
        {
            if (index < 0 || index >= _campaignLevels.Count)
            {
                Debug.LogError($"Level index {index} out of range!");
                return;
            }

            _currentLevelIndex = index;
            LevelData data = _campaignLevels[index];

            if (data == null)
            {
                Debug.LogWarning($"Level at index {index} is missing or null! Skipping...");
                return;
            }
            
            Debug.Log($"Loading Level: {data.LevelName}");
            BuildLevel(data);
            ApplyAtmosphere(data);
            ResetPlayer();
        }

        public void LoadNextLevel()
        {
            LoadLevel(_currentLevelIndex + 1);
        }

        public void RestartLevel()
        {
            LoadLevel(_currentLevelIndex);
        }

        private void BuildLevel(LevelData data)
        {
            ClearCurrentLevel();

            Vector3 spawnPosition = _startPosition;

            foreach (var segmentPrefab in data.Segments)
            {
                if (segmentPrefab == null) continue;

                // Use Zenject to instantiate so Injection works on LevelExit
                GameObject segmentObj = _container.InstantiatePrefab(segmentPrefab, spawnPosition, Quaternion.identity, _levelContainer);
                _currentSegments.Add(segmentObj);

                LevelSegment segment = segmentObj.GetComponent<LevelSegment>();
                if (segment != null && segment.EndPoint != null)
                {
                    spawnPosition = segment.EndPoint.position;
                }
                else
                {
                    // Fallback width if no EndPoint defined
                    spawnPosition += Vector3.right * 20f; 
                }
            }
        }

        private void ClearCurrentLevel()
        {
            foreach (var obj in _currentSegments)
            {
                if (obj != null) Destroy(obj);
            }
            _currentSegments.Clear();
        }

        private void ApplyAtmosphere(LevelData data)
        {
            if (_globalLight != null)
            {
                _globalLight.color = data.AtmosphereColor;
                _globalLight.intensity = data.GlobalLightIntensity;
            }

            if (_player != null)
            {
                // Optional: Adjust gravity per level
                // _player.SetGravityScale(data.GravityMultiplier); 
                // Note: Player controls gravity via state machine, so this might need a 'BaseGravity' modifier in controller.
                // For now, ignoring gravity overrides to keep consistency.
            }
        }

        private void ResetPlayer()
        {
            if (_player != null)
            {
                // Default safe spot
                Vector3 spawnPos = _startPosition + Vector3.up * 2f;

                // Try to find a spawn point in the first segment
                if (_currentSegments.Count > 0 && _currentSegments[0] != null)
                {
                    Transform spawnPoint = _currentSegments[0].transform.Find("SpawnPoint");
                    if (spawnPoint != null)
                    {
                        spawnPos = spawnPoint.position;
                    }
                    else
                    {
                        // Fallback: Start of segment + up
                        spawnPos = _currentSegments[0].transform.position + Vector3.up * 2f;
                    }
                }

                _player.transform.position = spawnPos;
                _player.SetVelocity(Vector2.zero);
            }
        }
    }
}
