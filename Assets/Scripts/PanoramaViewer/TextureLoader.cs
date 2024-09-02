using System.IO;
using UnityEngine;

namespace Assets.Scripts.PanoramaViewer
{
    public static class TextureLoader
    {
        public static Texture2D LoadTextureFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        public static RenderTexture ImageToRenderTexture(string path)
        {
            var texture = LoadTextureFromFile(path);
            var renderTexture = new RenderTexture(
                texture.width, texture.height, 32
            );
            Graphics.Blit(texture, renderTexture);
            return renderTexture;
        }
    }
}
