using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public class ScreenMessage
    {
        TextMeshPro textComponent;
        GameObject canvasWrapper;
        Canvas uiCanvas;
        Image imageComponent;

        public void Show() { canvasWrapper.SetActive(true); }

        public void Hide() { canvasWrapper.SetActive(false); }

        public void SetText(string text) { textComponent.text = text; }

        public void SetBackgroundColor(Color color) { imageComponent.color = color; }

        public ScreenMessage(Camera mainCamera)
        {
            canvasWrapper = new($"Screen Message {Guid.NewGuid()}");
            uiCanvas = canvasWrapper.AddComponent<Canvas>();
            canvasWrapper.AddComponent<CanvasScaler>();
            canvasWrapper.AddComponent<GraphicRaycaster>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiCanvas.worldCamera = mainCamera;
            uiCanvas.planeDistance = 1;

            // Text background
            Vector2 canvasSize = uiCanvas.GetComponent<RectTransform>().sizeDelta;
            GameObject background = new("Background");
            imageComponent = background.AddComponent<Image>();
            RectTransform imageTransform = background.GetComponent<RectTransform>();

            imageComponent.transform.SetParent(canvasWrapper.transform, false);
            imageTransform.sizeDelta = new Vector2(canvasSize.x * 1.5f, canvasSize.y * 1.5f);
            imageTransform.anchoredPosition3D = new Vector3(0, 0, 1);
            imageComponent.color = Color.clear;

            // Message text
            GameObject textWrapper = new("Text Wrapper");
            textWrapper.transform.SetParent(canvasWrapper.transform, false);
            textComponent = textWrapper.AddComponent<TextMeshPro>();
            textComponent.fontSharedMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");

            RectTransform textTransform = textWrapper.GetComponent<RectTransform>();
            textTransform.sizeDelta = new Vector2(canvasSize.x / 4, canvasSize.y);
            textComponent.fontSize = canvasSize.y * 20 / 100;
            textComponent.alignment = TextAlignmentOptions.Center;
        }
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

        /// <summary> Fades the skybox in or out by adjusting its exposure over time </summary>
        /// <param name="fadeIn">True for fade-in, False for fade-out.</param>
        /// <param name="duration">The duration of the transition in seconds.</param>
        /// <returns>An IEnumerator that can be used to yield control during the transition.</returns>
        public static IEnumerator SkyboxFadeTransition(bool fadeIn, float duration)
        {
            const float Steps = 100;
            float i = fadeIn ? 0 : Steps;
            while (fadeIn ? i <= Steps : i >= 0)
            {
                i = fadeIn ? i + 1 : i - 1;
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