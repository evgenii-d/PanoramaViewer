using System;

namespace Assets.Scripts.PanoramaViewer
{
    [Serializable]
    public class MinimapConfig
    {
        public MinimapPosition position = MinimapPosition.BottomRight;
        public int offset = 50;
        public float scale = 1.0f;
    }

    [Serializable]
    public class ViewerConfig
    {
        public bool autoPlay = true;
        public float imageDisplayTime = 15f;
        public float fadeDuration = 2f;
        public MinimapConfig minimap = new();

    }
}