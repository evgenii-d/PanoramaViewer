using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.PanoramaViewer
{
    public enum MinimapPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public static class Minimap
    {

        /// <summary>
        /// Adjusts the minimap size, position, and scale
        /// relative to the screen size and specified settings.
        /// </summary>
        /// <param name="rawImage">
        /// The RawImage component of the minimap to be adjusted.
        /// </param>
        /// <param name="position">
        /// The corner of the screen where the minimap should be positioned.
        /// </param>
        /// <param name="scale">
        /// A float value representing the scale of the minimap 
        /// starting from a quarter of the screen size.
        /// A value of 1.0 represents 100% (quarter size), 
        /// 2.0 represents 200% (half size), and 0.0 represents 0%.
        /// </param>
        /// <param name="padding">
        /// A float value for the padding (indent) from the screen edges.
        /// </param>
        public static void SetMinimapSizeAndPosition(
            GameObject minimap,
            MinimapPosition position,
            float scale,
            float padding
        )
        {
            var rawImage = minimap.transform.Find("Minimap Image");
            var screenSize = new Vector2(Screen.width, Screen.height);
            var baseMinimapSize = screenSize / 4f;
            var minimapSize = baseMinimapSize * Math.Clamp(scale, 0f, 2f);
            var rectTransform = rawImage.GetComponent<RectTransform>();
            rectTransform.sizeDelta = minimapSize;

            // Set the pivot point of the minimap
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Set anchor and position based on the chosen corner
            // (including uniform padding)
            switch (position)
            {
                case MinimapPosition.TopLeft:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.anchoredPosition = new Vector2(
                        minimapSize.x / 2 + padding,
                        -minimapSize.y / 2 - padding
                    );
                    break;

                case MinimapPosition.TopRight:
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.anchoredPosition = new Vector2(
                        -minimapSize.x / 2 - padding,
                        -minimapSize.y / 2 - padding
                    );
                    break;

                case MinimapPosition.BottomLeft:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.anchoredPosition = new Vector2(
                        minimapSize.x / 2 + padding,
                        minimapSize.y / 2 + padding
                    );
                    break;

                case MinimapPosition.BottomRight:
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.anchoredPosition = new Vector2(
                        -minimapSize.x / 2 - padding,
                        minimapSize.y / 2 + padding
                    );
                    break;
            }
        }
    }
}