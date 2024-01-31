using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PanoramaViewer
{
    [Serializable]
    public class ViewerSettings
    {
        public bool autoPlay = true;
        public float imageDelay = 15f;
        public float fadeDuration = 2f;
        public List<string> imageFormats = new() { ".jpg", ".png" };
        public List<string> videoFormats = new() { ".mp4", ".webm" };
    }

    public static class FileOperations
    {
        public static List<string> GetFilesFromDir(string dirPath, List<string> extensions = null)
        {
            List<string> files = Directory.GetFiles(dirPath).ToList();
            if (extensions == null) return files;
            return files.Where(file => file.Contains(Path.GetExtension(file).ToLower())).ToList();
        }
    }

    public static class PanoramicSkyboxControl
    {
        static Texture2D LoadTextureFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        static int FindGCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        static string CalculateAspectRatio(int width, int height)
        {
            int gcd = FindGCD(width, height);
            return $"{width / gcd}:{height / gcd}";
        }

        /// <summary>Transition effect using skybox exposure</summary>
        /// <param name="type">"fadeIn" or "fadeOut"</param>
        /// <param name="duration">Time in seconds</param>
        public static IEnumerator SkyboxFadeTransition(string type, float duration)
        {
            type = type.ToLower();
            const float Steps = 100;
            float i = type == "fadein" ? 0 : Steps;
            while (type == "fadein" ? i <= Steps : i >= 0)
            {
                i = type == "fadein" ? i + 1 : i - 1;
                RenderSettings.skybox.SetFloat("_Exposure", i / Steps);
                yield return new WaitForSeconds(duration / Steps);
            }
        }

        public static RenderTexture ImageToRenderTexture(string path)
        {
            Texture2D texture = LoadTextureFromFile(path);
            RenderTexture renderTexture = new(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);
            return renderTexture;
        }

        public static void UpdateSkyboxMainTexture(RenderTexture renderTexture)
        {
            RenderSettings.skybox.mainTexture = renderTexture;

            switch (CalculateAspectRatio(renderTexture.width, renderTexture.height))
            {
                case "2:1":
                    RenderSettings.skybox.DisableKeyword("_MAPPING_6_FRAMES_LAYOUT");
                    RenderSettings.skybox.SetFloat("_Mapping", 1);
                    RenderSettings.skybox.SetFloat("_ImageType", 0);
                    RenderSettings.skybox.SetFloat("_Layout", 0);
                    break;
                case "1:1":
                    RenderSettings.skybox.DisableKeyword("_MAPPING_6_FRAMES_LAYOUT");
                    RenderSettings.skybox.SetFloat("_Mapping", 1);
                    RenderSettings.skybox.SetFloat("_ImageType", 0);
                    RenderSettings.skybox.SetFloat("_Layout", 2);
                    break;
                case "6:1":
                    RenderSettings.skybox.EnableKeyword("_MAPPING_6_FRAMES_LAYOUT");
                    RenderSettings.skybox.SetFloat("_Mapping", 0);
                    break;
                default:
                    break;
            }
        }
    }
}