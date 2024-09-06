using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.PanoramaViewer.TextureLoader;

namespace Assets.Scripts.PanoramaViewer
{
    public enum MinimapPosition { TopLeft, TopRight, BottomLeft, BottomRight }

    public class Minimap
    {
        readonly GameObject minimap = new("Minimap");
        readonly RawImage minimapImage;
        readonly RectTransform imageTransform;
        readonly CanvasGroup canvasGroup;

        public Minimap(string path = null)
        {
            var minimapCanvas = minimap.AddComponent<Canvas>();
            minimapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            minimap.AddComponent<CanvasScaler>();
            minimap.AddComponent<GraphicRaycaster>();

            var minimapImageWrapper = new GameObject("Minimap Image");
            minimapImageWrapper.transform.SetParent(minimap.transform);
            minimapImage = minimapImageWrapper.AddComponent<RawImage>();
            imageTransform = minimapImage.GetComponent<RectTransform>();

            var screenSize = new Vector2(Screen.width, Screen.height);
            imageTransform.sizeDelta = screenSize / 4f;

            // CanvasGroup for fading
            canvasGroup = minimap.AddComponent<CanvasGroup>();

            SetPosition(MinimapPosition.BottomRight);
            if (path != null) SetImage(path);
        }

        public void Show() => canvasGroup.alpha = 1;
        public void Hide() => canvasGroup.alpha = 0;

        public bool IsVisible() => canvasGroup.alpha > 0;

        public void SetImage(string path)
        {
            var texture = LoadImageAsTexture(path);
            minimapImage.texture = texture;

            // Reset sizeDelta based on the screen size
            var screenSize = new Vector2(Screen.width, Screen.height);
            imageTransform.sizeDelta = screenSize / 4f;

            // Recalculate aspect ratio and adjust sizeDelta
            var aspectRatio = (float)texture.width / texture.height;
            var newSize = imageTransform.sizeDelta;

            if (aspectRatio > 1) // Wider image
            {
                newSize.y = newSize.x / aspectRatio;
            }
            else // Taller image or square
            {
                newSize.x = newSize.y * aspectRatio;
            }
            imageTransform.sizeDelta = newSize;
        }

        public void Scale(float scale)
        {
            scale = Mathf.Clamp(scale, 0, 2);
            imageTransform.localScale = new Vector3(scale, scale, 1);
        }

        public void SetPosition(MinimapPosition position, float offset = 0)
        {
            Vector2 anchorMin, anchorMax, pivot, offsetValue;
            switch (position)
            {
                case MinimapPosition.TopLeft:
                    anchorMin = anchorMax = pivot = new Vector2(0, 1);
                    offsetValue = new(offset, -offset);
                    break;

                case MinimapPosition.TopRight:
                    anchorMin = anchorMax = pivot = new Vector2(1, 1);
                    offsetValue = new(-offset, -offset);
                    break;

                case MinimapPosition.BottomLeft:
                    anchorMin = anchorMax = pivot = new Vector2(0, 0);
                    offsetValue = new(offset, offset);
                    break;

                case MinimapPosition.BottomRight:
                    anchorMin = anchorMax = pivot = new Vector2(1, 0);
                    offsetValue = new(-offset, offset);
                    break;
                default:
                    return;
            }
            imageTransform.anchorMin = anchorMin;
            imageTransform.anchorMax = anchorMax;
            imageTransform.pivot = pivot;
            imageTransform.anchoredPosition = offsetValue;
        }

        // public IEnumerator MinimapFadeTransition(bool fadeIn, float duration)
        public IEnumerator FadeTransition(bool fadeIn, float duration)
        {
            float start = fadeIn ? 0 : 1;
            float end = fadeIn ? 1 : 0;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float normalizedTime = t / duration;
                canvasGroup.alpha = Mathf.Lerp(start, end, normalizedTime);
                yield return null;
            }
            canvasGroup.alpha = end;
        }
    }
}