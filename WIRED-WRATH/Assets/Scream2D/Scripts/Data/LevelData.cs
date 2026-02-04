using UnityEngine;
using System.Collections.Generic;

namespace Scream2D.Data
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Scream2D/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Meta")]
        public string LevelName = "Level 1";
        [TextArea] public string Description = "A grim start...";

        [Header("Level Layout")]
        [Tooltip("Drag LevelSegment prefabs here in order.")]
        public List<GameObject> Segments;

        [Header("Atmosphere")]
        public Color AtmosphereColor = Color.gray;
        public float GlobalLightIntensity = 0.5f;
        public float GravityMultiplier = 1f;

        [Header("Audio")]
        public AudioClip BackgroundMusic;
    }
}
