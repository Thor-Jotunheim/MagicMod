using UnityEngine;
using UnityEngine.UI;

namespace MagicMod
{
    public class BombTimerUI : MonoBehaviour
    {
        private static GameObject bombPanel;
        private static Text bombText;

        private static GameObject defusePanel;
        private static Text defuseText;

        public static void ShowBombTimer(float totalTime)
        {
            if (bombPanel == null) CreateBombPanel();
            bombPanel.SetActive(true);
            bombText.text = $"Bomb: {totalTime:F0}s";
        }

        public static void UpdateBombTimer(float timeLeft)
        {
            if (bombPanel == null) return;
            if (!bombPanel.activeSelf) bombPanel.SetActive(true);
            bombText.text = $"Bomb: {timeLeft:F0}s";
        }

        public static void HideBombTimer()
        {
            if (bombPanel != null)
            {
                bombPanel.SetActive(false);
            }
        }

        public static void ShowDefuseTimer(float totalTime)
        {
            if (defusePanel == null) CreateDefusePanel();
            defusePanel.SetActive(true);
            defuseText.text = $"Defusing... {totalTime:F0}s";
        }

        public static void UpdateDefuseTimer(float timeLeft)
        {
            if (defusePanel == null) return;
            if (!defusePanel.activeSelf) defusePanel.SetActive(true);
            defuseText.text = $"Defusing... {timeLeft:F0}s";
        }

        public static void HideDefuseTimer()
        {
            if (defusePanel != null)
            {
                defusePanel.SetActive(false);
            }
        }

        private static void CreateBombPanel()
        {
            var canvasObj = new GameObject("BombTimerCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            bombPanel = new GameObject("BombPanel");
            bombPanel.transform.SetParent(canvasObj.transform, false);

            var rt = bombPanel.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 50);
            rt.anchoredPosition = new Vector2(0, 200);

            // Text
            var textObj = new GameObject("BombText");
            textObj.transform.SetParent(bombPanel.transform, false);
            bombText = textObj.AddComponent<Text>();
            bombText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
            bombText.alignment = TextAnchor.MiddleCenter;
            bombText.color = Color.red;
            bombText.text = "Bomb: 45";

            var textRT = textObj.GetComponent<RectTransform>();
            textRT.sizeDelta = new Vector2(300, 50);
            textRT.anchoredPosition = Vector2.zero;

            bombPanel.SetActive(false);
        }

        private static void CreateDefusePanel()
        {
            var canvasObj = new GameObject("DefuseTimerCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            defusePanel = new GameObject("DefusePanel");
            defusePanel.transform.SetParent(canvasObj.transform, false);

            var rt = defusePanel.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 50);
            rt.anchoredPosition = new Vector2(0, 150);

            // Text
            var textObj = new GameObject("DefuseText");
            textObj.transform.SetParent(defusePanel.transform, false);
            defuseText = textObj.AddComponent<Text>();
            defuseText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
            defuseText.alignment = TextAnchor.MiddleCenter;
            defuseText.color = Color.cyan;
            defuseText.text = "Defusing... 10s";

            var textRT = textObj.GetComponent<RectTransform>();
            textRT.sizeDelta = new Vector2(300, 50);
            textRT.anchoredPosition = Vector2.zero;

            defusePanel.SetActive(false);
        }
    }
}