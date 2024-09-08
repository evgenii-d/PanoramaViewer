using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PanoramaViewer
{
    public static class PanoramicSkyboxControl
    {
        /// <summary>
        /// Finds the greatest common divisor (GCD) of two integers 
        /// using the Euclidean algorithm.
        /// </summary>
        /// <param name="a">The first integer.</param>
        /// <param name="b">The second integer.</param>
        /// <returns>The greatest common divisor of a and b.</returns>
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

        /// <summary>
        /// Calculates the aspect ratio of a texture 
        /// by simplifying the width and height.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>
        /// A string representing the aspect ratio 
        /// in the format "width:height".
        /// </returns>
        static string CalculateAspectRatio(int width, int height)
        {
            int gcd = FindGCD(width, height);
            return $"{width / gcd}:{height / gcd}";
        }

        /// <summary>
        /// Fades the skybox in or out by adjusting its exposure over time.
        /// </summary>
        /// <param name="fadeIn">
        /// True for fade-in, False for fade-out.
        /// </param>
        /// <param name="duration">
        /// The duration in seconds.
        /// </param>
        /// <returns>IEnumerator for use in a coroutine.</returns>
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

        /// <summary>
        /// Updates the skybox's main texture 
        /// and adjusts the skybox mapping settings
        /// based on the aspect ratio of the provided render texture.
        /// </summary>
        /// <param name="renderTexture">
        /// The texture to apply to the skybox.
        /// </param>
        public static void UpdateSkyboxMainTexture(RenderTexture renderTexture)
        {
            RenderSettings.skybox.mainTexture = renderTexture;
            var mappingKeyword = "_MAPPING_6_FRAMES_LAYOUT";
            var aspectRation = CalculateAspectRatio(
                renderTexture.width, renderTexture.height
            );

            switch (aspectRation)
            {
                case "2:1": // Equirectangular format
                    RenderSettings.skybox.DisableKeyword(mappingKeyword);
                    RenderSettings.skybox.SetFloat("_Mapping", 1);
                    RenderSettings.skybox.SetFloat("_ImageType", 0);
                    RenderSettings.skybox.SetFloat("_Layout", 0);
                    break;
                case "1:1": // 180° fisheye format
                    RenderSettings.skybox.DisableKeyword(mappingKeyword);
                    RenderSettings.skybox.SetFloat("_Mapping", 1);
                    RenderSettings.skybox.SetFloat("_ImageType", 0);
                    RenderSettings.skybox.SetFloat("_Layout", 2);
                    break;
                case "6:1": // 6-frame cubemap format
                    RenderSettings.skybox.EnableKeyword(mappingKeyword);
                    RenderSettings.skybox.SetFloat("_Mapping", 0);
                    break;
                default:
                    break;
            }
        }
    }
}