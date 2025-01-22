using UnityEngine;
using UnityEngine.UI;  // ✅ UI Elements (Text, Image, etc.)

namespace MagicMod
{
    public class ItemStandTimer : MonoBehaviour
    {
        private static GameObject timerUI;
        private static Text timerText;
        private static Image timerBar;

        public static void ShowPlacementTimer()
        {
            if (timerUI == null)
            {
                CreateUI();
            }
            timerUI.SetActive(true);
        }

        public static void UpdatePlacementTimer(float timeLeft)
        {
            if (timerUI == null) return;

            timerText.text = $"Placing... {timeLeft:F1}s";
            timerBar.fillAmount = timeLeft / 3.8f; // Normalize the bar
        }

        public static void HidePlacementTimer()
        {
            if (timerUI != null)
            {
                timerUI.SetActive(false);
            }
        }

        private static void CreateUI()
        {
            timerUI = new GameObject("PlacementTimer");
            Canvas canvas = timerUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // ✅ Ensure CanvasScaler is added to scale UI properly
            CanvasScaler scaler = timerUI.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject textObj = new GameObject("TimerText");
            textObj.transform.SetParent(timerUI.transform);
            timerText = textObj.AddComponent<Text>();
            timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            timerText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();  // ✅ Fix Missing RectTransform
            textRect.anchoredPosition = new Vector2(0, -50);

            GameObject barObj = new GameObject("TimerBar");
            barObj.transform.SetParent(timerUI.transform);
            timerBar = barObj.AddComponent<Image>();
            timerBar.color = Color.green;
            
            RectTransform barRect = barObj.AddComponent<RectTransform>();  // ✅ Fix Missing RectTransform
            barRect.sizeDelta = new Vector2(200, 20);
            barRect.anchoredPosition = new Vector2(0, -80);

            timerUI.SetActive(false);
        }
    }
}