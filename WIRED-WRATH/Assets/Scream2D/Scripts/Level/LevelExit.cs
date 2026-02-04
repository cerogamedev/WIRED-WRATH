using UnityEngine;
using Zenject;
using Scream2D.Systems;

namespace Scream2D.Level
{
    public class LevelExit : MonoBehaviour
    {
        private LevelManager _levelManager;

        [Inject]
        public void Construct(LevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Use tag or check component
            if (other.GetComponent<Controllers.PlayerController>() != null)
            {
                Debug.Log("Level Complete! Loading Next...");
                _levelManager.LoadNextLevel();
            }
        }
    }
}
