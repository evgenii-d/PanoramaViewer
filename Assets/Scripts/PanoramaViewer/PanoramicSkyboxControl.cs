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
        public static IEnumerator SkyboxFadeTransition(
            bool fadeIn, float duration
        )
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

        public static void UpdateSkyboxMainTexture(RenderTexture renderTexture)
        {
            RenderSettings.skybox.mainTexture = renderTexture;
            var aspectRation = CalculateAspectRatio(
                renderTexture.width, renderTexture.height
            );

            switch (aspectRation)
            {
                case "2:1":
                    RenderSettings.skybox.DisableKeyword(
                        "_MAPPING_6_FRAMES_LAYOUT"
                    );
                    RenderSettings.skybox.SetFloat("_Mapping", 1);
                    RenderSettings.skybox.SetFloat("_ImageType", 0);
                    RenderSettings.skybox.SetFloat("_Layout", 0);
                    break;
                case "1:1":
                    RenderSettings.skybox.DisableKeyword(
                        "_MAPPING_6_FRAMES_LAYOUT"
                    );
                    RenderSettings.skybox.SetFloat("_Mapping", 1);
                    RenderSettings.skybox.SetFloat("_ImageType", 0);
                    RenderSettings.skybox.SetFloat("_Layout", 2);
                    break;
                case "6:1":
                    RenderSettings.skybox.EnableKeyword(
                        "_MAPPING_6_FRAMES_LAYOUT"
                    );
                    RenderSettings.skybox.SetFloat("_Mapping", 0);
                    break;
                default:
                    break;
            }
        }
    }
}