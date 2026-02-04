using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace Scream2D.Editor
{
    public class AudioGenerator
    {
        [MenuItem("Scream2D/Audio/Generate Retro Blip")]
        public static void GenerateBlip()
        {
            string folderPath = "Assets/Scream2D/Audio";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, "TextBlip.wav");
            
            // Audio Specs
            int sampleRate = 44100;
            float duration = 0.05f; // 50ms
            int sampleCount = (int)(sampleRate * duration);
            float frequency = 440f;
            
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // Square Wave with decay
                float wave = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * frequency * t));
                // Simple Linear Decay
                float vol = 1.0f - (t / duration);
                samples[i] = wave * vol * 0.5f; // Master volume 0.5
            }

            WriteWavFile(filePath, samples, sampleRate);
            
            AssetDatabase.Refresh();
            
            // Select it to confirm
            Object clip = AssetDatabase.LoadAssetAtPath<Object>(filePath);
            Selection.activeObject = clip;
            EditorGUIUtility.PingObject(clip);
            
            Debug.Log($"✅ Generated Blip Sound: {filePath}");
        }

        private static void WriteWavFile(string filepath, float[] samples, int sampleRate)
        {
            using (var fileStream = new FileStream(filepath, FileMode.Create))
            using (var writer = new BinaryWriter(fileStream))
            {
                int bitsPerSample = 16;
                int byteRate = sampleRate * 1 * bitsPerSample / 8;
                int blockAlign = 1 * bitsPerSample / 8;

                // RIFF chunk
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + samples.Length * 2);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                // fmt chunk
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1Size (16 for PCM)
                writer.Write((short)1); // AudioFormat (1 = PCM)
                writer.Write((short)1); // NumChannels (Mono)
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)blockAlign);
                writer.Write((short)bitsPerSample);

                // data chunk
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(samples.Length * 2);

                // Write Data
                foreach (float sample in samples)
                {
                    short shortSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                    writer.Write(shortSample);
                }
            }
        }
    }
}
