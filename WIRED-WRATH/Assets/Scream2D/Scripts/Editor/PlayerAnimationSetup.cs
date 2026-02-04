using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using Scream2D.Controllers;

namespace Scream2D.Editor
{
    public class PlayerAnimationSetup : UnityEditor.Editor
    {
        [MenuItem("Scream2D/Setup Player Animations")]
        public static void SetupAnimations()
        {
            string animationsDir = "Assets/Scream2D/Animations";
            if (!AssetDatabase.IsValidFolder(animationsDir))
            {
                AssetDatabase.CreateFolder("Assets/Scream2D", "Animations");
            }

            string controllerPath = $"{animationsDir}/PlayerController.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                Debug.Log($"Created AnimatorController at {controllerPath}");
            }

            // Find Clara.aseprite imported clips
            string asepritePath = "Assets/ASEPRITE-FILES/Clara.aseprite";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(asepritePath);
            
            Debug.Log($"Searching for assets in {asepritePath}. Found {assets.Length} sub-assets.");
            foreach(var asset in assets) Debug.Log($"- Asset: {asset.name} ({asset.GetType().Name})");

            AnimationClip walkClip = assets.OfType<AnimationClip>().FirstOrDefault(c => c.name.ToLower().Contains("walk"));
            if (walkClip == null) walkClip = assets.OfType<AnimationClip>().FirstOrDefault();

            AnimationClip jumpUpClip = assets.OfType<AnimationClip>().FirstOrDefault(c => c.name.ToLower().Contains("jump-up"));
            AnimationClip jumpDownClip = assets.OfType<AnimationClip>().FirstOrDefault(c => c.name.ToLower().Contains("jump-down"));

            Sprite frame0 = assets.OfType<Sprite>().FirstOrDefault(s => s.name.Contains("Frame_0"));
            string idleClipPath = $"{animationsDir}/Clara_Idle.anim";
            AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPath);

            if (idleClip == null && frame0 != null)
            {
                idleClip = new AnimationClip();
                idleClip.name = "Clara_Idle";
                
                // Create a single frame animation
                EditorCurveBinding curveBinding = new EditorCurveBinding();
                curveBinding.type = typeof(SpriteRenderer);
                curveBinding.path = "";
                curveBinding.propertyName = "m_Sprite";

                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[1];
                keyframes[0] = new ObjectReferenceKeyframe();
                keyframes[0].time = 0f;
                keyframes[0].value = frame0;

                AnimationUtility.SetObjectReferenceCurve(idleClip, curveBinding, keyframes);
                
                AssetDatabase.CreateAsset(idleClip, idleClipPath);
                Debug.Log($"Created static Idle animation at {idleClipPath}");
            }

            if (controller != null && controller.layers.Length > 0)
            {
                if (walkClip != null) AddStateToController(controller, "Walk", walkClip);
                if (idleClip != null) AddStateToController(controller, "Idle", idleClip);
                if (jumpUpClip != null) AddStateToController(controller, "JumpUp", jumpUpClip);
                if (jumpDownClip != null) AddStateToController(controller, "JumpDown", jumpDownClip);
                
                Debug.Log($"Assigned states. Walk: {(walkClip != null ? walkClip.name : "None")}, Idle: {(idleClip != null ? idleClip.name : "None")}, JumpUp: {(jumpUpClip != null ? jumpUpClip.name : "None")}, JumpDown: {(jumpDownClip != null ? jumpDownClip.name : "None")}");
            }
            else
            {
                Debug.LogError("AnimatorController is missing or has no layers!");
            }

            // Assign to Player in Scene
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                Animator animator = player.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = player.gameObject.AddComponent<Animator>();
                }
                animator.runtimeAnimatorController = controller;

                // Fix Visuals: Reset color to white
                SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;

                // Add Light2D for better visibility
                SetupLight(player.gameObject);
                
                // Add Camera Controller
                SetupCamera();

                // Fix Jitter: Enable Interpolation
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                // Fix Floating: Adjust Collider to match sprite height & pivot
                AdjustCollider(player, walkClip);

                EditorUtility.SetDirty(player);
                Debug.Log("✅ Animator, Color, Light and Collider adjusted for Player in scene.");
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerController not found in scene. Please open the main game scene.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SetupLight(GameObject playerGo)
        {
            // Note: Light2D requires UnityEngine.Rendering.Universal
            // If the project doesn't have it, this might fail, but manifest.json confirms URP.
            var light = playerGo.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
            if (light == null)
            {
                GameObject lightGo = new GameObject("PlayerLight");
                lightGo.transform.SetParent(playerGo.transform);
                lightGo.transform.localPosition = new Vector3(0, 1f, 0); // Above feet
                
                light = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
                light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
                light.pointLightInnerRadius = 0.5f;
                light.pointLightOuterRadius = 3f;
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.95f, 0.8f); // Warm white
                Debug.Log("✅ Added Light2D to Player.");
            }
        }

        private static void SetupCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                var controller = mainCam.GetComponent<Systems.CameraController>();
                if (controller == null)
                {
                    controller = mainCam.gameObject.AddComponent<Systems.CameraController>();
                }
                
                controller.DefaultOrthographicSize = 3.5f;
                mainCam.orthographicSize = 3.5f;
                Debug.Log("✅ Added CameraController to Main Camera.");
            }
        }

        private static void AdjustCollider(PlayerController player, AnimationClip walkClip)
        {
            // Get sprite dimensions from the first frame of walk if possible
            // Or just hardcode based on Clara (19x61) if we can't find it
            float height = 0.61f; // Default for Clara (61 pixels at 100 PPU)
            float width = 0.22f;  // Default for Clara (22 pixels max width)

            // Try to find a sprite to get real bounds
            string asepritePath = "Assets/ASEPRITE-FILES/Clara.aseprite";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(asepritePath);
            Sprite sprite = assets.OfType<Sprite>().FirstOrDefault();
            if (sprite != null)
            {
                height = sprite.rect.height / sprite.pixelsPerUnit;
                width = sprite.rect.width / sprite.pixelsPerUnit;
            }

            var box = player.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = new Vector2(width, height);
                // With Pivot at Bottom, offset Y should be half-height
                box.offset = new Vector2(0, height / 2f);
                Debug.Log($"Adjusted BoxCollider2D: Size({box.size.x}, {box.size.y}), Offset({box.offset.x}, {box.offset.y})");
            }
            
            var capsule = player.GetComponent<CapsuleCollider2D>();
            if (capsule != null)
            {
                capsule.size = new Vector2(width, height);
                capsule.offset = new Vector2(0, height / 2f);
                Debug.Log($"Adjusted CapsuleCollider2D: Size({capsule.size.x}, {capsule.size.y}), Offset({capsule.offset.x}, {capsule.offset.y})");
            }
        }

        private static void AddStateToController(AnimatorController controller, string stateName, AnimationClip clip)
        {
            var rootStateMachine = controller.layers[0].stateMachine;
            var existingState = rootStateMachine.states.FirstOrDefault(s => s.state.name == stateName);
            
            if (existingState.state == null)
            {
                var state = rootStateMachine.AddState(stateName);
                state.motion = clip;
            }
            else
            {
                existingState.state.motion = clip;
            }
        }

        private static AnimationClip FindClip(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"{name} t:AnimationClip");
            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            return null;
        }
    }
}
