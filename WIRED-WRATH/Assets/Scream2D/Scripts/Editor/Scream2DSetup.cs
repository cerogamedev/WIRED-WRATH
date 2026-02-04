using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;
using Zenject;
using Scream2D.Controllers;
using Scream2D.Installers;
using Scream2D.Systems;

namespace Scream2D.Editor
{
    [InitializeOnLoad]
    public class Scream2DSetup : EditorWindow
    {
        static Scream2DSetup()
        {
            EditorApplication.delayCall += () => {
                if (!SessionState.GetBool("Scream2DSetupFinal", false))
                {
                    string targetScene = "Assets/Scream2D/Scenes/MainGame_New.unity";
                    if (EditorSceneManager.GetActiveScene().path != targetScene)
                    {
                        EditorSceneManager.OpenScene(targetScene);
                        Debug.Log("Switched to MainGame scene.");
                    }
                    
                    SetupScene();
                    SessionState.SetBool("Scream2DSetupFinal", true);
                }
            };
        }

        [MenuItem("Scream2D/Setup Current Scene")]
        public static void SetupScene()
        {
            // 1. Setup Zenject SceneContext
            SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
            if (sceneContext == null)
            {
                GameObject contextObj = new GameObject("SceneContext");
                sceneContext = contextObj.AddComponent<SceneContext>();
                Debug.Log("Created Zenject SceneContext.");
            }

            // 2. Setup Installer
            GameInstaller installer = FindFirstObjectByType<GameInstaller>();
            if (installer == null)
            {
                GameObject installerObj = new GameObject("GameInstaller");
                installer = installerObj.AddComponent<GameInstaller>();
                Debug.Log("Created GameInstaller.");
            }
            
            // Link installer to context
            if (sceneContext.Installers == null || sceneContext.Installers.Count() == 0)
            {
                sceneContext.Installers = new System.Collections.Generic.List<MonoInstaller> { installer };
            }

            // 3. Setup Ground
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                ground = new GameObject("Ground");
                ground.transform.position = new Vector3(0, -4, 0);
                ground.transform.localScale = new Vector3(20, 2, 1);
                
                var renderer = ground.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                renderer.color = new Color(0.3f, 0.1f, 0.1f); 
                renderer.sortingOrder = -1;
                
                ground.AddComponent<BoxCollider2D>();
                ground.AddComponent<BoxCollider2D>();
                
                int groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer != -1) ground.layer = groundLayer;
                
                Debug.Log("Created Ground.");
            }

            // 3b. Setup global lighting for visibility
            var globalLight = FindFirstObjectByType<Light2D>();
            if (globalLight == null)
            {
                GameObject lightObj = new GameObject("GlobalLight");
                globalLight = lightObj.AddComponent<Light2D>();
                globalLight.lightType = Light2D.LightType.Global;
                globalLight.intensity = 0.5f;
                Debug.Log("Created Global Light 2D.");
            }

            // 4. Setup Player
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                player.transform.position = new Vector3(0, 1, 0);
                
                player.AddComponent<SpriteRenderer>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                player.GetComponent<SpriteRenderer>().color = Color.cyan;
                
                player.AddComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
                
                player.AddComponent<BoxCollider2D>();
                var playerController = player.GetComponent<PlayerController>();
                if (playerController == null) playerController = player.AddComponent<PlayerController>();
                
                Debug.Log("Created Player.");
            }

            // 5. Setup ScreamMeter
            GameObject systems = GameObject.Find("Systems");
            if (systems == null)
            {
                systems = new GameObject("Systems");
                systems.AddComponent<ScreamMeter>();
                systems.AddComponent<ScreamAtmosphereController>();
                systems.AddComponent<LevelManager>();
                Debug.Log("Created Scream Systems.");
            }
            
            ground.AddComponent<BreathingEffect>();

            // 8. Setup Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.05f);
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }
        }

        [MenuItem("Scream2D/Add Test Wall")]
        public static void CreateWall()
        {
            GameObject wall = new GameObject("Wall_Test");
            wall.transform.position = new Vector3(5, 0, 0);
            wall.transform.localScale = new Vector3(1, 10, 1);
            
            var renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            renderer.color = new Color(0.2f, 0.2f, 0.2f);
            
            wall.AddComponent<BoxCollider2D>();
            
            int wallLayer = LayerMask.NameToLayer("Wall");
            if (wallLayer != -1)
            {
                wall.layer = wallLayer;
                Debug.Log("Created Wall on 'Wall' layer.");
            }
            else
            {
                Debug.LogWarning("Layer 'Wall' not found.");
            }

            Selection.activeGameObject = wall;
            SceneView.FrameLastActiveSceneView();
        }
    }
}
