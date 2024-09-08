using System.IO;
using UnityEngine;

namespace Assets.Scripts.PanoramaViewer
{
    /// <summary>
    /// The TextureLoader class provides utility methods for loading
    /// images as textures or render textures in Unity.
    /// </summary>
    public static class TextureLoader
    {
        /// <summary>
        /// Loads an image file from the given path and returns it
        /// as a Texture2D object.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <returns>
        /// A Texture2D object if the file exists, or null if the file
        /// could not be found.
        /// </returns>
        public static Texture2D LoadImageAsTexture(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning("File Not Found: " + path);
                return null;
            }
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        /// <summary>
        /// Converts an image file to a RenderTexture.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <returns>
        /// A RenderTexture created from the image.
        /// The width and height of the RenderTexture
        /// match the dimensions of the image.
        /// </returns>
        public static RenderTexture ImageToRenderTexture(string path)
        {
            var texture = LoadImageAsTexture(path);
            var renderTexture = new RenderTexture(
                texture.width, texture.height, 32
            );
            Graphics.Blit(texture, renderTexture);
            return renderTexture;
        }
    }
}
