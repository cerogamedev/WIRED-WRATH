using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Scream2D.Level;
using Scream2D.Data;
using Scream2D.Systems;

namespace Scream2D.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        private GameObject _levelRoot;
        private string _levelName = "New_Level_01";
        private bool _autoAddToManager = true;

        [MenuItem("Scream2D/Level Editor (Advanced)")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("🛠️ Level Designer", EditorStyles.boldLabel);

            // --- Section 1: Draft Management ---
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("1. Setup", EditorStyles.miniLabel);
            _levelRoot = (GameObject)EditorGUILayout.ObjectField("Level Root", _levelRoot, typeof(GameObject), true);
            
            if (_levelRoot == null)
            {
                if (GUILayout.Button("Create New Level Draft"))
                {
                    CreateDraft();
                }
                EditorGUILayout.HelpBox("Select or Create a root object to start building.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            // --- Section 2: Palette ---
            if (_levelRoot != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("2. Palette (Auto-Adds to Root)", EditorStyles.boldLabel);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Blocks
                GUILayout.Label("Blocks & Hazards", EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🟩 Ground")) CreateGround();
                if (GUILayout.Button("🎨 Tilemap Setup")) SetupTilemapInDraft();
                if (GUILayout.Button("🧱 Wall")) CreateWall();
                if (GUILayout.Button("🔺 Spikes")) CreateSpikes();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                DrawEnemyPalette();
                EditorGUILayout.Space(5);

                // Logic
                GUILayout.Label("Logic", EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("📍 Spawn Point")) CreateSpawnPoint();
                if (GUILayout.Button("🚪 Exit Gate")) CreateExit();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();

                // --- Section 3: Publishing ---
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("3. Publish", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                _levelName = EditorGUILayout.TextField("Level Name", _levelName);
                _autoAddToManager = EditorGUILayout.Toggle("Auto Add to Manager", _autoAddToManager);

                if (GUILayout.Button("💾 Save & Publish Level", GUILayout.Height(30)))
                {
                    SaveLevel();
                }
                EditorGUILayout.EndVertical();
            }
        }

        #region Actions

        private void CreateDraft()
        {
            _levelRoot = new GameObject("Level_Draft");
            _levelRoot.transform.position = Vector3.zero;
            _levelRoot.AddComponent<LevelSegment>(); // Prepare component
            
            // Create default EndPoint
            GameObject ep = new GameObject("EndPoint");
            ep.transform.parent = _levelRoot.transform;
            ep.transform.localPosition = new Vector3(20, 0, 0);
            _levelRoot.GetComponent<LevelSegment>().EndPoint = ep.transform;

            Selection.activeGameObject = _levelRoot;
            Debug.Log("Created new Level Draft.");
        }

        private void CreateGround()
        {
            GameObject ground = CreateSpriteObject("Ground", Color.white);
            ground.transform.localScale = new Vector3(5, 1, 1);
            AssignLayer(ground, "Ground");
            PlaceObject(ground);
        }

        private void CreateWall()
        {
            GameObject wall = CreateSpriteObject("Wall", Color.gray);
            wall.transform.localScale = new Vector3(1, 4, 1);
            AssignLayer(wall, "Wall");
            PlaceObject(wall);
        }

        private void CreateSpikes()
        {
            GameObject spike = CreateSpriteObject("Spikes", Color.red);
            spike.transform.localScale = new Vector3(1, 0.5f, 1);
            
            // Add BioHazard
            spike.AddComponent<BioHazard>();
            
            AssignLayer(spike, "Default"); 
            PlaceObject(spike);
        }

        private void CreateSpawnPoint()
        {
            if (_levelRoot.transform.Find("SpawnPoint") != null)
            {
                Debug.LogWarning("SpawnPoint already exists!");
                Selection.activeGameObject = _levelRoot.transform.Find("SpawnPoint").gameObject;
                return;
            }

            GameObject sp = new GameObject("SpawnPoint");
            PlaceObject(sp);
            sp.transform.localPosition = new Vector3(0, 2, 0); // Override place logic to be safer
            
            // Visual for Editor only (Icon)
            // No sprite needed, just an empty object is fine for logic
        }

        private void CreateExit()
        {
             GameObject exit = CreateSpriteObject("LevelExit", new Color(0, 1, 0, 0.5f));
             exit.transform.localScale = new Vector3(1, 3, 1);
             exit.GetComponent<BoxCollider2D>().isTrigger = true;
             exit.AddComponent<LevelExit>();
             
             PlaceObject(exit);
        }

        private void SetupTilemapInDraft()
        {
            if (_levelRoot == null) CreateNewLevel();

            // 1. Create/Find Grid inside root
            Transform gridT = _levelRoot.transform.Find("Grid");
            Grid grid = null;
            if (gridT == null)
            {
                GameObject gridGo = new GameObject("Grid");
                gridGo.transform.SetParent(_levelRoot.transform);
                gridGo.transform.localPosition = Vector3.zero;
                grid = gridGo.AddComponent<Grid>();
            }
            else
            {
                grid = gridT.GetComponent<Grid>();
            }

            // 2. Setup Layers using the centralized utility
            GameObject groundGo = TilemapSetup.SetupLayers(grid.transform);
            Selection.activeGameObject = groundGo;
        }

        private void CreateNPC()
        {
            if (_levelRoot == null) CreateNewLevel();

            GameObject npc = CreateSpriteObject("NPC_Talker", Color.green); // Green Placeholder
            npc.transform.localScale = Vector3.one;
            
            // Interaction Trigger
            var collider = npc.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(3, 3); // Large trigger area

            // Logic
            npc.AddComponent<Scream2D.Level.DialogueTrigger>();

            PlaceObject(npc);
            Debug.Log("Created NPC Talker.");
        }

        #endregion

        #region Helpers

        private GameObject CreateSpriteObject(string name, Color color)
        {
            if (_levelRoot == null) CreateNewLevel();

            GameObject obj = new GameObject(name);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetSquareSprite();
            sr.color = color;
            
            obj.AddComponent<BoxCollider2D>(); // Auto-fits to sprite
            return obj;
        }

        private void CreateEnemyObject<T>(string name, Color color) where T : Component
        {
            if (_levelRoot == null) CreateNewLevel();

            GameObject obj = new GameObject(name);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetSquareSprite();
            sr.color = color;
            
            obj.AddComponent<BoxCollider2D>(); // Hitbox
            obj.AddComponent<Rigidbody2D>().gravityScale = 0; // Most are flying/kinematic base
            obj.AddComponent<T>(); // Sticky Logic

            // Parenting
            GameObject enemyGroup = GetChildGroup("Enemies");
            obj.transform.SetParent(enemyGroup.transform);
            obj.transform.localPosition = Vector3.zero;

            // Place near camera
            PlaceObject(obj, false); // Don't reparent to root, we already parented to group

            Selection.activeGameObject = obj;
            Undo.RegisterCreatedObjectUndo(obj, $"Create {name}");
        }

        private GameObject GetChildGroup(string name)
        {
             if (_levelRoot == null) return null;
             Transform t = _levelRoot.transform.Find(name);
             if (t == null)
             {
                 GameObject g = new GameObject(name);
                 g.transform.SetParent(_levelRoot.transform);
                 g.transform.localPosition = Vector3.zero;
                 return g;
             }
             return t.gameObject;
        }

        private Sprite _cachedSquareSprite;
        private Sprite GetSquareSprite()
        {
            if (_cachedSquareSprite != null) return _cachedSquareSprite;

            // Try load existing
            string path = "Assets/Scream2D/Textures/Square.png";
            _cachedSquareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            
            if (_cachedSquareSprite == null)
            {
                // Create Texture
                EnsureDirectory("Assets/Scream2D/Textures");
                Texture2D texture = new Texture2D(32, 32);
                Color[] pixels = new Color[32 * 32];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                texture.SetPixels(pixels);
                texture.Apply();

                System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.Refresh();

                // Import as Sprite
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32; // So 32px = 1 Unit size
                    importer.filterMode = FilterMode.Point;
                    importer.SaveAndReimport();
                }

                _cachedSquareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            return _cachedSquareSprite;
        }

        private void PlaceObject(GameObject obj, bool parentToRoot = true)
        {
            if (parentToRoot && _levelRoot != null)
            {
                obj.transform.parent = _levelRoot.transform;
            }
            
            // Place near Scene View camera or center
            if (SceneView.lastActiveSceneView != null)
            {
                Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
                obj.transform.position = new Vector3(camPos.x, camPos.y, 0);
            }
            else
            {
                obj.transform.localPosition = Vector3.zero;
            }

            Selection.activeGameObject = obj;
            Undo.RegisterCreatedObjectUndo(obj, $"Create {obj.name}");
        }
        
        private void CreateNewLevel()
        {
            CreateDraft();
        }

        private void DrawEnemyPalette()
        {
            GUILayout.Label("Enemies", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Oculus (Eye)", GUILayout.Height(40))) CreateEnemyObject<Scream2D.Enemies.OculusNull>("Oculus-Null", Color.yellow);
            if (GUILayout.Button("Hertz (Jam)", GUILayout.Height(40))) CreateEnemyObject<Scream2D.Enemies.HertzHound>("Hertz-Hound", new Color(0.6f, 0f, 0.8f)); // Purple
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Echo (Ghost)", GUILayout.Height(40))) CreateEnemyObject<Scream2D.Enemies.EchoUnit>("Echo-Unit", new Color(0, 1, 1, 0.5f)); // Cyan
            if (GUILayout.Button("Crab (Tank)", GUILayout.Height(40))) CreateEnemyObject<Scream2D.Enemies.ServerCrab>("Server-Crab", new Color(0.8f, 0.2f, 0.2f)); // Red
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Bit-Fly (Swarm)", GUILayout.Height(30))) CreateEnemyObject<Scream2D.Enemies.BitFly>("Bit-Fly", Color.green);
            
            EditorGUILayout.Space(5);
            if (GUILayout.Button("🗣️ NPC Talker", GUILayout.Height(30))) CreateNPC();
        }

        private void AssignLayer(GameObject obj, string layerName)
        {
             // no-op or impl
             int layer = LayerMask.NameToLayer(layerName);
             if (layer != -1) obj.layer = layer;
        }

        private void SaveLevel()
        {
            if (_levelRoot == null) return;
            
            // 1. Validation
            EnsureDirectory("Assets/Scream2D/Prefabs/Segments");
            EnsureDirectory("Assets/Scream2D/Data/Levels");

            if (_levelRoot.GetComponent<LevelSegment>() == null) _levelRoot.AddComponent<LevelSegment>();

            // 2. Prefab
            string prefabPath = $"Assets/Scream2D/Prefabs/Segments/{_levelName}.prefab";
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(_levelRoot, prefabPath, InteractionMode.UserAction);
            Debug.Log($"Saved Prefab: {prefabPath}");

            // 3. Data
            LevelData data = ScriptableObject.CreateInstance<LevelData>();
            data.LevelName = _levelName;
            data.Segments = new List<GameObject> { prefab };
            data.AtmosphereColor = RenderSettings.ambientLight;

            string dataPath = $"Assets/Scream2D/Data/Levels/{_levelName}.asset";
            dataPath = AssetDatabase.GenerateUniqueAssetPath(dataPath);
            AssetDatabase.CreateAsset(data, dataPath);
            Debug.Log($"Saved Data: {dataPath}");

            // 4. Manager
            if (_autoAddToManager)
            {
                LevelManager manager = Object.FindFirstObjectByType<LevelManager>();
                if (manager != null)
                {
                    SerializedObject so = new SerializedObject(manager);
                    SerializedProperty prop = so.FindProperty("_campaignLevels");
                    int index = prop.arraySize;
                    prop.InsertArrayElementAtIndex(index);
                    prop.GetArrayElementAtIndex(index).objectReferenceValue = data;
                    so.ApplyModifiedProperties();
                    Debug.Log("Level added only Manager campaign!");
                }
            }
            
            EditorUtility.DisplayDialog("Success", $"Level '{_levelName}' Published!", "Awesome");
        }

        private void EnsureDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
        }

        #endregion
    }
}
