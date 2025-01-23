using UnityEngine;
using UnityEngine.UI;

namespace MagicMod
{
    public class ItemStandTimer : MonoBehaviour
    {
        private static GameObject timerUI;
        private static Text timerText;

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

            // Update text countdown
            // e.g. "Placing... 3.4s"
            timerText.text = $"Placing... {timeLeft:F1}s";
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
            // Create parent UI Canvas
            timerUI = new GameObject("PlacementTimer");
            Canvas canvas = timerUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = timerUI.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            timerUI.AddComponent<GraphicRaycaster>();

            // Create Panel
            GameObject panel = new GameObject("TimerPanel");
            panel.transform.SetParent(timerUI.transform);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(220, 60);
            panelRect.anchoredPosition = new Vector2(0, -50);

            // Create Timer Text
            GameObject textObj = new GameObject("TimerText");
            textObj.transform.SetParent(panel.transform);
            timerText = textObj.AddComponent<Text>();
            timerText.text = "Placing... 3.8s";
            timerText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            timerText.alignment = TextAnchor.MiddleCenter;
            timerText.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(200, 30);
            textRect.anchoredPosition = new Vector2(0, 30);

            // Hide UI initially
            timerUI.SetActive(false);
        }
    }
}