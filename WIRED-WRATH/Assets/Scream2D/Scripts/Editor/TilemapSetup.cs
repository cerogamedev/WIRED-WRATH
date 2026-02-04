using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace Scream2D.Editor
{
    public class TilemapSetup : UnityEditor.Editor
    {
        [MenuItem("Scream2D/Setup Tilemap System")]
        public static void SetupTilemap()
        {
            // 1. Create/Find Grid
            Grid grid = Object.FindFirstObjectByType<Grid>();
            if (grid == null)
            {
                GameObject gridGo = new GameObject("Grid");
                grid = gridGo.AddComponent<Grid>();
                Debug.Log("Created new Grid object.");
            }

            Selection.activeGameObject = SetupLayers(grid.transform);
            Debug.Log("✅ Tilemap System Ready! You can now use the Tile Palette to paint on 'Ground' and 'Background' layers.");
        }

        public static GameObject SetupLayers(Transform parent)
        {
            GameObject groundGo = GetOrCreateLayer(parent, "Ground", 0, true);
            GetOrCreateLayer(parent, "Background", -10, false);
            return groundGo;
        }

        private static GameObject GetOrCreateLayer(Transform parent, string name, int sortingOrder, bool hasCollision)
        {
            Transform t = parent.Find(name);
            Tilemap tilemap = null;
            if (t == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent);
                go.transform.localPosition = Vector3.zero;
                
                tilemap = go.AddComponent<Tilemap>();
                var renderer = go.AddComponent<TilemapRenderer>();
                renderer.sortingOrder = sortingOrder;

                if (name == "Ground") go.layer = LayerMask.NameToLayer("Ground");

                if (hasCollision)
                {
                    var collider = go.AddComponent<TilemapCollider2D>();
                    var composite = go.AddComponent<CompositeCollider2D>();
                    var rb = go.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.bodyType = RigidbodyType2D.Static;
                    collider.usedByComposite = true;
                }
                
                Debug.Log($"Created '{name}' Tilemap.");
                return go;
            }
            return t.gameObject;
        }
    }
}
