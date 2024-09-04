using System;
using System.Collections.Generic;
using Assets.Scripts.PanoramaViewer;

[Serializable]
public class Minimap
{
    public MinimapPosition position = MinimapPosition.BottomRight;
    public float scale = 1.0f;
}

[Serializable]
public class ViewerConfig
{
    public bool autoPlay = true;
    public float imageDelay = 15f;
    public float fadeDuration = 2f;
    public Minimap minimap = new();

}