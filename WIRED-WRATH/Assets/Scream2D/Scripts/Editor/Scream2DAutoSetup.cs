using UnityEngine;
using UnityEditor;
using System.IO;

namespace Scream2D.Editor
{
    public class Scream2DAutoSetup : UnityEditor.Editor
    {
        [MenuItem("Scream2D/Auto-Setup VFX & Assets")]
        public static void SetupVFX()
        {
            EnsureDirectories();
            
            // 1. Create Textures
            Sprite pulseSprite = CreateSimpleSprite("PulseCircle", Color.white, DrawCircle);
            Sprite glitchSprite = CreateSimpleSprite("GlitchNoise", Color.cyan, DrawNoise);
            Sprite impactSprite = CreateSimpleSprite("ImpactStar", Color.white, DrawStar);
            Sprite virusSprite = CreateSimpleSprite("VirusCross", Color.green, DrawCross);

            // 2. Create Particle Prefabs
            GameObject pulsePrefab = CreateParticlePrefab("VFX_ScreamPulse", pulseSprite, Color.white, 1.5f, 0.5f);
            GameObject glitchPrefab = CreateParticlePrefab("VFX_GlitchStep", glitchSprite, Color.cyan, 0.5f, 0.2f);
            GameObject impactPrefab = CreateParticlePrefab("VFX_GroundPound", impactSprite, new Color(1f, 0.8f, 0.8f), 1f, 0.4f);
            GameObject virusPrefab = CreateParticlePrefab("VFX_VirusExplosion", virusSprite, Color.green, 0.8f, 0.6f);

            // 3. Assign to ParticleFactory
            var factory = Object.FindFirstObjectByType<Systems.ParticleFactory>();
            if (factory != null)
            {
                Undo.RecordObject(factory, "Assign VFX");
                SerializedObject so = new SerializedObject(factory);
                
                so.FindProperty("_screamPulse").objectReferenceValue = pulsePrefab.GetComponent<ParticleSystem>();
                so.FindProperty("_glitchEffect").objectReferenceValue = glitchPrefab.GetComponent<ParticleSystem>();
                so.FindProperty("_groundPoundImpact").objectReferenceValue = impactPrefab.GetComponent<ParticleSystem>();
                so.FindProperty("_virusExplosion").objectReferenceValue = virusPrefab.GetComponent<ParticleSystem>();
                
                so.ApplyModifiedProperties();
                Debug.Log("✅ Assigned VFX to ParticleFactory scene object.");
            }
            else
            {
                Debug.LogWarning("⚠️ ParticleFactory not found in scene. Skipping assignment.");
            }

            // 4. Assign Logic Virus Prefab to Player
            AssignPlayerAssets();

            // 5. Assign Oculus Projectile
            AssignEnemyAssets();

            Debug.Log("✨ Auto-Setup Complete! VFX & Projectiles Ready.");
        }

        private static void AssignPlayerAssets()
        {
            var player = Object.FindFirstObjectByType<Controllers.PlayerController>();
            if (player != null)
            {
                // Create Logic Virus Projectile Prefab if missing
                string path = "Assets/Scream2D/Prefabs/Projectiles/LogicVirus.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null)
                {
                    EnsureDirectory("Assets/Scream2D/Prefabs/Projectiles");
                    GameObject go = new GameObject("LogicVirus");
                    go.AddComponent<SpriteRenderer>().sprite = CreateSimpleSprite("VirusProj", Color.green, DrawCross);
                    go.AddComponent<BoxCollider2D>().isTrigger = true;
                    go.AddComponent<Projectiles.LogicVirusProjectile>();
                    
                    prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                    Object.DestroyImmediate(go);
                }

                SerializedObject so = new SerializedObject(player);
                so.FindProperty("LogicVirusPrefab").objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
                Debug.Log("✅ Assigned LogicVirusPrefab to Player.");
            }
        }

        private static void AssignEnemyAssets()
        {
            string path = "Assets/Scream2D/Prefabs/Projectiles/EnemyProjectile.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                EnsureDirectory("Assets/Scream2D/Prefabs/Projectiles");
                GameObject go = new GameObject("EnemyProjectile");
                go.AddComponent<SpriteRenderer>().sprite = CreateSimpleSprite("EnemyProj", Color.red, DrawCircle);
                go.AddComponent<CircleCollider2D>().isTrigger = true;
                go.AddComponent<Projectiles.EnemyProjectile>();
                
                prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                Object.DestroyImmediate(go);
            }

            var enemies = Object.FindObjectsByType<Enemies.OculusNull>(FindObjectsSortMode.None);
            foreach (var oculus in enemies)
            {
                SerializedObject so = new SerializedObject(oculus);
                so.FindProperty("projectilePrefab").objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
            }
            if (enemies.Length > 0) Debug.Log($"✅ Assigned EnemyProjectile to {enemies.Length} Oculus-Null enemies.");
        }

        #region Helpers

        private static void EnsureDirectories()
        {
            EnsureDirectory("Assets/Scream2D/Textures/VFX");
            EnsureDirectory("Assets/Scream2D/Prefabs/VFX");
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private static Sprite CreateSimpleSprite(string name, Color color, System.Action<Texture2D> drawFunc)
        {
            string path = $"Assets/Scream2D/Textures/VFX/{name}.png";
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            Texture2D tex = new Texture2D(64, 64);
            // Clear
            Color[] clear = new Color[64*64];
            for(int i=0; i<clear.Length; i++) clear[i] = Color.clear;
            tex.SetPixels(clear);

            drawFunc(tex);
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateParticlePrefab(string name, Sprite sprite, Color color, float size, float duration)
        {
            string path = $"Assets/Scream2D/Prefabs/VFX/{name}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = new GameObject(name);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = duration;
            main.startLifetime = duration;
            main.startSize = size;
            main.startColor = color;
            main.loop = false;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.enabled = true;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.mainTexture = sprite.texture;

            // Destroy self after play
            // We can't add script logic here easily without a script helper, 
            // but ParticleSystem has 'Stop Action -> Destroy' in newer Unity versions.
            main.stopAction = ParticleSystemStopAction.Destroy;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // Draw Funcs
        private static void DrawCircle(Texture2D tex)
        {
            Vector2 center = new Vector2(32, 32);
            float radius = 30;
            for (int x=0; x<64; x++) {
                for(int y=0; y<64; y++) {
                    if (Vector2.Distance(new Vector2(x,y), center) < radius) tex.SetPixel(x,y, Color.white);
                }
            }
        }
        private static void DrawSquare(Texture2D tex)
        {
            for (int x=16; x<48; x++) for(int y=16; y<48; y++) tex.SetPixel(x,y, Color.white);
        }
        private static void DrawCross(Texture2D tex)
        {
             for (int i=0; i<64; i++) {
                 tex.SetPixel(i, i, Color.white);
                 tex.SetPixel(i, 63-i, Color.white);
                 tex.SetPixel(i+1, i, Color.white); // Thicken
                 tex.SetPixel(i, 63-i-1, Color.white);
             }
        }
        private static void DrawStar(Texture2D tex) => DrawCross(tex); // Reuse for now
        private static void DrawNoise(Texture2D tex)
        {
            for (int x=0; x<64; x++) {
                for(int y=0; y<64; y++) {
                    if (Random.value > 0.8f) tex.SetPixel(x,y, Color.white);
                }
            }
        }

        #endregion
    }
}
