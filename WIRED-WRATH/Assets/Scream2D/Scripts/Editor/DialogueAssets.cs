using UnityEngine;
using UnityEditor;
using System.IO;

namespace Scream2D.Editor
{
    public class DialogueAssets
    {
        [MenuItem("Scream2D/UI/Generate UI Background")]
        public static void GenerateBackground()
        {
            string folderPath = "Assets/Scream2D/Textures/UI";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, "DialogueBox.png");
            
            int width = 64;
            int height = 64;
            int border = 4;
            
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            
            Color bgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Deep Blue-Black
            Color borderColor = new Color(0.8f, 0.8f, 0.9f, 1f); // White-Blue
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x < border || x >= width - border || y < border || y >= height - border;
                    pixels[y * width + x] = isBorder ? borderColor : bgColor;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            
            AssetDatabase.Refresh();
            
            // Configure Import Settings for 9-Slice
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Point; // Pixel Perfect
                importer.spriteBorder = new Vector4(border, border, border, border); // 9-Slice
                importer.SaveAndReimport();
            }
            
            Debug.Log($"✅ Generated UI Background: {filePath}");
        }
    }
}
