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
            // Cleanup existing
            GameObject existing = GameObject.Find("DialogueCanvas");
            if (existing != null) Object.DestroyImmediate(existing);

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
            
            // 3. Create Panel (Blue-Black Box with Border)
            GameObject panelGo = new GameObject("SpeakingPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            
            Image panelImage = panelGo.AddComponent<Image>();
            
            // Try load 9-slice sprite
            string bgPath = "Assets/Scream2D/Textures/UI/DialogueBox.png";
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            
            if (bgSprite == null)
            {
                // Auto-generate if missing
                DialogueAssets.GenerateBackground();
                bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            }

            if (bgSprite != null)
            {
                panelImage.sprite = bgSprite;
                panelImage.type = Image.Type.Sliced;
                panelImage.pixelsPerUnitMultiplier = 1; 
            }
            else
            {
                panelImage.color = new Color(0, 0, 0, 0.9f);
            }
            
            RectTransform panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.05f); // Bottom Center
            panelRect.anchorMax = new Vector2(0.9f, 0.05f); 
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(0, 100); 
            panelRect.anchoredPosition = new Vector2(0, 50);

            // Add Layout Components
            VerticalLayoutGroup layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 30); // More padding for border
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
            tmp.fontSize = 32; // Bigger pixel font
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.enableWordWrapping = true;
            
            // Try Load Font (VT323)
            // Note: User might need to create TMP Asset manually, but we try standard or skip
            // 1. Try Load Custom Pixel Font (Priority)
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Scream2D/Fonts/VT323-Regular SDF.asset");
            
            // 2. If missing, fallback to Default (Safety)
            if (font == null)
            {
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

            if (font != null) tmp.font = font;

            // 5. Create "Click to Continue" Indicator
            GameObject indicatorGo = new GameObject("Indicator");
            indicatorGo.transform.SetParent(panelGo.transform, false);
            
            RectTransform indRect = indicatorGo.AddComponent<RectTransform>();
            indRect.anchorMin = new Vector2(1, 0);
            indRect.anchorMax = new Vector2(1, 0);
            indRect.pivot = new Vector2(1, 0);
            indRect.sizeDelta = new Vector2(100, 30);
            
            // Layout Element to ignore layout group (or handle it)
            // Actually, if we are in a vertical layout group, this will be stacked below text. 
            // We want it overlaying or floating.
            // Better: Create a Wrapper Panel for Layout, but for now let's just use LayoutElement ignore
            LayoutElement le = indicatorGo.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            
            indRect.anchoredPosition = new Vector2(-10, 10); // Bottom Right padding

            TextMeshProUGUI indTmp = indicatorGo.AddComponent<TextMeshProUGUI>();
            indTmp.text = "Left Click ▼";
            indTmp.fontSize = 20;
            if (font != null) indTmp.font = font;
            indTmp.color = Color.yellow;
            indTmp.alignment = TextAlignmentOptions.BottomRight;

            // 6. Connect References
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("_panel").objectReferenceValue = panelGo;
            so.FindProperty("_textMesh").objectReferenceValue = tmp;
            so.FindProperty("_speakingIndicator").objectReferenceValue = indicatorGo;
            // DialogueUI.cs defines: [SerializeField] private Image _speakingIndicator;
            // But we made it Text!
            // I should update DialogueUI.cs to accept GameObject or Component, or just change it to GameObject in code.
            // For now, let's keep it clean. I need to update DialogueUI.cs to type 'GameObject' for indicator.
            
            // Audio Source
            AudioSource audio = managerGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            so.FindProperty("_audioSource").objectReferenceValue = audio;
            so.FindProperty("_typingSpeed").floatValue = 0.035f;
            
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Scream2D/Audio/TextBlip.wav");
            if (clip != null) so.FindProperty("_typingSound").objectReferenceValue = clip;
            
            so.ApplyModifiedProperties();

            // 7. Hide initially
            panelGo.SetActive(false);
            
            Selection.activeGameObject = managerGo;
            Debug.Log("✅ Dialogue UI Created (Polished).");
        }
    }
}
