using System;
using System.Collections.Generic;

namespace Assets.Scripts.PanoramaViewer
{
    [Serializable]
    public class MinimapConfig
    {
        public string minimapFile;
        public string panoramaFile;
        public MinimapPosition position = MinimapPosition.BottomRight;
        public float scale = 1.0f;
        public int xOffset = 50;
        public int yOffset = 50;
        public int zOffset = 0;
    }

    [Serializable]
    public class ViewerConfig
    {
        public bool autoPlay = true;
        public float imageDisplayTime = 15f;
        public float fadeDuration = 2f;
        public List<MinimapConfig> minimaps = new()
        {
            new MinimapConfig() {
                minimapFile = "FullFileNameFromMinimapsFolder",
                panoramaFile = "fullFileNameFromPanoramaMediaFilesFolder",
            }
        };

    }
}