using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PanoramaViewer
{
    public static class PanoramicSkyboxControl
    {
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

        /// <summary>
        /// Fades the skybox in or out by adjusting its exposure over time
        /// </summary>
        /// <param name="fadeIn">True for fade-in, False for fade-out</param>
        /// <param name="duration">The duration in seconds.</param>
        /// <returns>IEnumerator that can be used to yield control</returns>
        public static IEnumerator SkyboxFadeTransition(bool fadeIn, float duration)
        {
            float start = fadeIn ? 0 : 1;
            float end = fadeIn ? 1 : 0;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                var exposure = Mathf.Lerp(start, end, t / duration);
                RenderSettings.skybox.SetFloat("_Exposure", exposure);
                yield return null;
            }
            RenderSettings.skybox.SetFloat("_Exposure", end);
        }

        public static void UpdateSkyboxMainTexture(RenderTexture renderTexture)
        {
            RenderSettings.skybox.mainTexture = renderTexture;
            var mappingKeyword = "_MAPPING_6_FRAMES_LAYOUT";
            var aspectRation = CalculateAspectRatio(
                renderTexture.width, renderTexture.height
            );

            switch (aspectRation)
            {
                case "2:1":
                    RenderSettings.skybox.DisableKeyword(mappingKeyword);
                    RenderSettings.skybox.SetFloat("_Mapping", 1);
                    RenderSettings.skybox.SetFloat("_ImageType", 0);
                    RenderSettings.skybox.SetFloat("_Layout", 0);
                    break;
                case "1:1":
                    RenderSettings.skybox.DisableKeyword(mappingKeyword);
                    RenderSettings.skybox.SetFloat("_Mapping", 1);
                    RenderSettings.skybox.SetFloat("_ImageType", 0);
                    RenderSettings.skybox.SetFloat("_Layout", 2);
                    break;
                case "6:1":
                    RenderSettings.skybox.EnableKeyword(mappingKeyword);
                    RenderSettings.skybox.SetFloat("_Mapping", 0);
                    break;
                default:
                    break;
            }
        }
    }
}