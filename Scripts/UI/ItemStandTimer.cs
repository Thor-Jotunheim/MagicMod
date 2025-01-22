using UnityEngine;
using UnityEngine.UI;

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
            timerBar.fillAmount = Mathf.Clamp01(timeLeft / 3.8f); // Prevents overflow
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
            // ✅ Create parent UI Canvas
            timerUI = new GameObject("PlacementTimer");
            Canvas canvas = timerUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = timerUI.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            timerUI.AddComponent<GraphicRaycaster>(); // Ensures UI interaction works

            // ✅ Create Panel for UI elements
            GameObject panel = new GameObject("TimerPanel");
            panel.transform.SetParent(timerUI.transform);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(220, 60);
            panelRect.anchoredPosition = new Vector2(0, -50);

            // ✅ Create Timer Text
            GameObject textObj = new GameObject("TimerText");
            textObj.transform.SetParent(panel.transform);
            timerText = textObj.AddComponent<Text>();
            timerText.text = "Placing... 3.8s";
            timerText.font = Font.CreateDynamicFontFromOSFont("Arial", 16); // ✅ Safer font loading
            timerText.alignment = TextAnchor.MiddleCenter;
            timerText.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(200, 30);
            textRect.anchoredPosition = new Vector2(0, 10);

            // ✅ Create Progress Bar
            GameObject barObj = new GameObject("TimerBar");
            barObj.transform.SetParent(panel.transform);
            timerBar = barObj.AddComponent<Image>();
            timerBar.color = Color.green;

            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(200, 10);
            barRect.anchoredPosition = new Vector2(0, -20);

            // ✅ Make sure UI is hidden initially
            timerUI.SetActive(false);
        }
    }
}