using System;
using System.Collections.Generic;

[Serializable]
public class ViewerSettings
{
    public bool autoPlay = true;
    public float imageDelay = 15f;
    public float fadeDuration = 2f;
    public List<string> imageFormats = new() { ".jpg", ".png" };
    public List<string> videoFormats = new() { ".mp4", ".webm" };
}