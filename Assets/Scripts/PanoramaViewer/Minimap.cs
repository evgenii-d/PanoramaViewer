using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static Assets.Scripts.PanoramaViewer.TextureLoader;

namespace Assets.Scripts.PanoramaViewer
{
    public enum MinimapPosition { TopLeft, TopRight, BottomLeft, BottomRight }

    /// <summary>
    /// The Minimap class handles the creation, scaling,
    /// positioning, and fading of a minimap in Unity.
    /// The minimap displays an image or a video 
    /// and can be adjusted based on screen size and aspect ratio.
    /// </summary>
    public class Minimap
    {
        private readonly RawImage rawImage;
        private readonly VideoPlayer videoPlayer;
        private readonly CanvasGroup canvasGroup;
        private readonly RectTransform rawImageTransform;
        readonly List<string> imageFormats = new() { ".jpg", ".png" };
        readonly List<string> videoFormats = new() { ".mp4", ".webm" };

        public Minimap(Camera renderCamera, string path = null)
        {
            var minimap = new GameObject("Minimap");
            var minimapCanvas = minimap.AddComponent<Canvas>();
            minimapCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            minimapCanvas.worldCamera = renderCamera;
            minimapCanvas.planeDistance = 1;
            minimap.AddComponent<CanvasScaler>();
            minimap.AddComponent<GraphicRaycaster>();

            // Create and set up the RawImage for displaying the minimap
            var rawImageWrapper = new GameObject("RawImage Wrapper");
            rawImageWrapper.transform.SetParent(minimap.transform);
            rawImage = rawImageWrapper.AddComponent<RawImage>();
            rawImageTransform = rawImage.GetComponent<RectTransform>();
            rawImageTransform.anchoredPosition3D = new Vector3(0, 0, 0);

            // Set initial size of the minimap (1/4th of the screen size)
            var screenSize = new Vector2(Screen.width, Screen.height);
            rawImageTransform.sizeDelta = screenSize / 4f;

            // CanvasGroup for fading
            canvasGroup = minimap.AddComponent<CanvasGroup>();

            videoPlayer = rawImageWrapper.AddComponent<VideoPlayer>();
            videoPlayer.SetDirectAudioVolume(0, 0);
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;

            SetPosition(MinimapPosition.BottomRight);
            Scale(1);
            if (path != null) SetMedia(path);
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

        private void OnVideoPrepared(VideoPlayer player)
        {
            var renderTexture = new RenderTexture(
                (int)player.width, (int)player.height, 32
            );
            rawImage.texture = renderTexture;
            player.targetTexture = renderTexture;
            AdjustMinimapSize(renderTexture);
            player.Play();
        }

        /// <summary>
        /// Adjusts the size of the minimap based on the screen
        /// dimensions and the media's aspect ratio.
        /// </summary>
        /// <param name="texture">
        /// The texture of the minimap image.
        /// </param>
        private void AdjustMinimapSize(RenderTexture texture)
        {
            var screenSize = new Vector2(Screen.width, Screen.height);
            rawImageTransform.sizeDelta = screenSize / 4f;

            // Recalculate the sizeDelta to maintain the correct aspect ratio
            var aspectRatio = (float)texture.width / texture.height;
            var newSize = rawImageTransform.sizeDelta;

            // Adjust dimensions based on aspect ratio
            if (aspectRatio > 1) newSize.y = newSize.x / aspectRatio;
            else newSize.x = newSize.y * aspectRatio;
            rawImageTransform.sizeDelta = newSize;
        }

        /// <summary>
        /// Sets the media to be displayed on the minimap and adjusts
        /// its size based on screen dimensions and aspect ratio.
        /// </summary>
        /// <param name="path">File path to the media.</param>
        public void SetMedia(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Failed to load media from '{path}'");
                return;
            }
            var fileFormat = Path.GetExtension(path);

            videoPlayer.Stop();
            if (videoFormats.Contains(fileFormat))
            {
                videoPlayer.url = path;
                videoPlayer.Prepare();
            }
            else if (imageFormats.Contains(fileFormat))
            {
                var renderTexture = LoadImageAsRenderTexture(path);
                rawImage.texture = renderTexture;
                AdjustMinimapSize(renderTexture);
                Resources.UnloadUnusedAssets();
            }
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
            rawImageTransform.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>
        /// Sets the position of the minimap on the screen
        /// using predefined positions and an optional offset.
        /// </summary>
        /// <param name="position">
        /// The position of the minimap
        /// (TopLeft, TopRight, BottomLeft, BottomRight).
        /// </param>
        /// <param name="xOffset">X offset</param>
        /// <param name="yOffset">Y offset</param>
        /// <param name="zOffset">Z offset</param>
        public void SetPosition(
            MinimapPosition position,
            float xOffset = 0,
            float yOffset = 0,
            float zOffset = 0
        )
        {
            Vector2 anchorMin, anchorMax, pivot;
            Vector3 offsetValue;
            switch (position)
            {
                case MinimapPosition.TopLeft:
                    anchorMin = anchorMax = pivot = new Vector2(0, 1);
                    offsetValue = new(xOffset, -yOffset, zOffset);
                    break;

                case MinimapPosition.TopRight:
                    anchorMin = anchorMax = pivot = new Vector2(1, 1);
                    offsetValue = new(-xOffset, -yOffset, zOffset);
                    break;

                case MinimapPosition.BottomLeft:
                    anchorMin = anchorMax = pivot = new Vector2(0, 0);
                    offsetValue = new(xOffset, yOffset, zOffset);
                    break;

                case MinimapPosition.BottomRight:
                    anchorMin = anchorMax = pivot = new Vector2(1, 0);
                    offsetValue = new(-xOffset, yOffset, zOffset);
                    break;
                default:
                    return;
            }
            rawImageTransform.anchorMin = anchorMin;
            rawImageTransform.anchorMax = anchorMax;
            rawImageTransform.pivot = pivot;
            rawImageTransform.anchoredPosition3D = offsetValue;
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