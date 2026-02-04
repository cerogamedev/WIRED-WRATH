using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Scream2D.UI;

namespace Scream2D.Editor
{
    public class DialogueUISetup
    {
        [MenuItem("Scream2D/UI/Setup Dialogue UI")]
        public static void SetupDialogueUI()
        {
            // 1. Create Canvas
            GameObject canvasGo = new GameObject("DialogueCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 2. Create DialogueUI Manager
            GameObject managerGo = new GameObject("DialogueManager");
            managerGo.transform.SetParent(canvasGo.transform);
            DialogueUI ui = managerGo.AddComponent<DialogueUI>();
            
            // 3. Create Panel (Black Box)
            GameObject panelGo = new GameObject("SpeakingPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            
            Image panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.9f);
            
            RectTransform panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.05f); // Bottom Center Stretch
            panelRect.anchorMax = new Vector2(0.9f, 0.05f); // Pivot from bottom
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(0, 100); // Initial Height
            panelRect.anchoredPosition = new Vector2(0, 50);

            // Add Layout Components for Dynamic Resizing
            VerticalLayoutGroup layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = panelGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 4. Create Text
            GameObject textGo = new GameObject("DialogueText");
            textGo.transform.SetParent(panelGo.transform, false);
            
            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.color = Color.white;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.wrappingEnabled = true;

            // Important for auto-sizing
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.1f); 
            textRect.anchorMax = new Vector2(0.95f, 0.9f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // 5. Audio Source
            AudioSource audio = managerGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;

            // 6. Connect References via SerializedObject to access private fields
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("_panel").objectReferenceValue = panelGo;
            so.FindProperty("_textMesh").objectReferenceValue = tmp;
            so.FindProperty("_audioSource").objectReferenceValue = audio;
            so.FindProperty("_typingSpeed").floatValue = 0.05f;
            so.ApplyModifiedProperties();

            // 7. Hide initially
            panelGo.SetActive(false);

            Selection.activeGameObject = managerGo;
            Debug.Log("✅ Dialogue UI Created.");
        }
    }
}
