using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.PanoramaViewer.TextureLoader;

namespace Assets.Scripts.PanoramaViewer
{
    public enum MinimapPosition { TopLeft, TopRight, BottomLeft, BottomRight }

    /// <summary>
    /// The Minimap class handles the creation, scaling,
    /// positioning, and fading of a minimap in Unity.
    /// The minimap displays an image and can be adjusted
    /// based on screen size and aspect ratio.
    /// </summary>
    public class Minimap
    {
        readonly GameObject minimap = new("Minimap");
        readonly RawImage minimapImage;
        readonly RectTransform imageTransform;
        readonly CanvasGroup canvasGroup;

        public Minimap(string path = null)
        {
            // Create the canvas for the minimap, which will render on screen
            var minimapCanvas = minimap.AddComponent<Canvas>();
            minimapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            minimap.AddComponent<CanvasScaler>();
            minimap.AddComponent<GraphicRaycaster>();

            // Create and set up the RawImage for displaying the minimap image
            var minimapImageWrapper = new GameObject("Minimap Image");
            minimapImageWrapper.transform.SetParent(minimap.transform);
            minimapImage = minimapImageWrapper.AddComponent<RawImage>();
            imageTransform = minimapImage.GetComponent<RectTransform>();

            // Set initial size of the minimap (1/4th of the screen size)
            var screenSize = new Vector2(Screen.width, Screen.height);
            imageTransform.sizeDelta = screenSize / 4f;

            // CanvasGroup for fading
            canvasGroup = minimap.AddComponent<CanvasGroup>();

            // Set the minimap's default position
            SetPosition(MinimapPosition.BottomRight);

            // If a path is provided, set the minimap image
            if (path != null) SetImage(path);
        }

        /// <summary>
        /// Shows the minimap by setting the alpha value to 1.
        /// </summary>
        public void Show() => canvasGroup.alpha = 1;

        /// <summary>
        /// Hides the minimap by setting the alpha value to 0.
        /// </summary>
        public void Hide() => canvasGroup.alpha = 0;

        /// <summary>
        /// Checks whether the minimap is currently visible.
        /// </summary>
        public bool IsVisible() => canvasGroup.alpha > 0;

        /// <summary>
        /// Adjusts the size of the minimap based on the screen dimensions
        /// and the image's aspect ratio.
        /// </summary>
        /// <param name="texture">The texture of the minimap image.</param>
        private void AdjustMinimapSize(Texture texture)
        {
            var screenSize = new Vector2(Screen.width, Screen.height);
            imageTransform.sizeDelta = screenSize / 4f;

            // Recalculate the sizeDelta to maintain the correct aspect ratio
            var aspectRatio = (float)texture.width / texture.height;
            var newSize = imageTransform.sizeDelta;

            // Adjust dimensions based on aspect ratio
            if (aspectRatio > 1) newSize.y = newSize.x / aspectRatio;
            else newSize.x = newSize.y * aspectRatio;
            imageTransform.sizeDelta = newSize;
        }

        /// <summary>
        /// Sets the image to be displayed on the minimap and adjusts
        /// its size based on screen dimensions and aspect ratio.
        /// </summary>
        /// <param name="path">File path to the image.</param>
        public void SetImage(string path)
        {
            var texture = LoadImageAsTexture(path);
            if (texture == null)
            {
                Debug.LogWarning($"Failed to load texture from {path}");
                return;
            }
            minimapImage.texture = texture;
            AdjustMinimapSize(texture);
        }

        /// <summary>
        /// Scales the minimap by a given factor, 
        /// clamped between 0 and 2.
        /// </summary>
        /// <param name="scale">
        /// The scale factor to apply to the minimap.
        /// </param>
        public void Scale(float scale)
        {
            scale = Mathf.Clamp(scale, 0, 2);
            imageTransform.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>
        /// Sets the position of the minimap on the screen
        /// using predefined positions and an optional offset.
        /// </summary>
        /// <param name="position">
        /// The position of the minimap
        /// (TopLeft, TopRight, BottomLeft, BottomRight).
        /// </param>
        /// <param name="offset">
        /// Optional offset to apply to the minimap's position.
        /// </param>
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

        /// <summary>
        /// Performs a fade-in or fade-out transition
        /// on the minimap over the specified duration.
        /// </summary>
        /// <param name="fadeIn">
        /// True for fade-in, False for fade-out.
        /// </param>
        /// <param name="duration">
        /// The duration in seconds.
        /// </param>
        /// <returns>IEnumerator for use in a coroutine.</returns>
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