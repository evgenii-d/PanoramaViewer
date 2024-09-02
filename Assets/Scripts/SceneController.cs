using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Assets.Scripts.PanoramaViewer;
using static Assets.Scripts.PanoramaViewer.Minimap;
using static Assets.Scripts.PanoramaViewer.TextureLoader;
using static Assets.Scripts.PanoramaViewer.PanoramicSkyboxControl;

public class SceneController : MonoBehaviour
{
    public Camera mainCamera;
    ViewerSettings viewerSettings = new();
    ScreenMessage screenMessage;
    VideoPlayer videoPlayer;
    // List to store the paths of images and videos to display
    List<string> mediaFiles;
    // Tracks the index of the currently displayed media file
    int currentMediaIndex = -1;
    // Prevents transitions from happening while one is in progress
    bool transitionLock = true;
    // Flag to indicate if this is the initial run
    bool firstRun = true;

    public enum PanoramaDirection
    {
        Next,
        Previous
    }

    public static List<string> GetFilesFromDir(
            string dirPath, List<string> extensions = null
        )
    {
        List<string> files = Directory.GetFiles(dirPath).ToList();
        if (extensions == null) return files;
        return files.Where(
            file => file.Contains(Path.GetExtension(file).ToLower())
        ).ToList();
    }

    /// <summary> Handles keyboard input for navigation </summary>
    void ControlKeys()
    {
        var keys = new Dictionary<KeyCode, PanoramaDirection>
        {
            { KeyCode.LeftArrow, PanoramaDirection.Previous },
            { KeyCode.PageDown, PanoramaDirection.Previous },
            { KeyCode.RightArrow, PanoramaDirection.Next },
            { KeyCode.PageUp, PanoramaDirection.Next }
        };

        foreach (var element in keys)
        {
            if (Input.GetKeyDown(element.Key))
            {
                StartCoroutine(ChangePanorama(element.Value));
                break;
            }
        }
    }

    /// <summary>
    /// Waits for a specified time before fading out the skybox
    /// </summary>
    IEnumerator VideoFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(
            SkyboxFadeTransition(false, viewerSettings.fadeDuration)
        );
    }

    /// <summary> 
    /// Waits for the image display time before fading out
    /// </summary>
    IEnumerator ImageFadeOut()
    {
        yield return new WaitForSeconds(viewerSettings.imageDelay);
        yield return SkyboxFadeTransition(false, viewerSettings.fadeDuration);
        StartCoroutine(ChangePanorama(PanoramaDirection.Next));
    }

    /// <summary>
    /// Fades in the skybox and unlocks transitions
    /// </summary>
    IEnumerator UnlockTransition()
    {
        StartCoroutine(
            SkyboxFadeTransition(true, viewerSettings.fadeDuration)
        );
        yield return new WaitForSeconds(viewerSettings.fadeDuration);
        transitionLock = false;
    }

    void OnVideoPrepared(VideoPlayer _)
    {
        var newRenderTexture = new RenderTexture(
            (int)videoPlayer.width, (int)videoPlayer.height, 32
        );

        if (viewerSettings.autoPlay)
        {
            var fadeDuration = viewerSettings.fadeDuration - 1;
            var timeBeforeEnd = (float)videoPlayer.length - fadeDuration;
            StartCoroutine(VideoFadeOut(timeBeforeEnd));
        }

        videoPlayer.targetTexture = newRenderTexture;
        screenMessage.Hide();
        UpdateSkyboxMainTexture(newRenderTexture);
        StartCoroutine(UnlockTransition());
        videoPlayer.Play();
    }

    IEnumerator ChangePanorama(PanoramaDirection direction)
    {
        transitionLock = true;
        currentMediaIndex += direction == PanoramaDirection.Next ? 1 : -1;

        // Handles cycling back around if reaching end of the media file list
        currentMediaIndex = (
            currentMediaIndex + mediaFiles.Count
        ) % mediaFiles.Count;

        if (!viewerSettings.autoPlay && !firstRun)
        {
            yield return SkyboxFadeTransition(
                false, viewerSettings.fadeDuration
            );
        }

        string fileFormat = Path.GetExtension(mediaFiles[currentMediaIndex]);
        if (viewerSettings.videoFormats.Contains(fileFormat))
        {
            videoPlayer.url = mediaFiles[currentMediaIndex];
            videoPlayer.Prepare();
        }
        else if (viewerSettings.imageFormats.Contains(fileFormat))
        {
            videoPlayer.Stop();
            var renderTexture = ImageToRenderTexture(
                mediaFiles[currentMediaIndex]
            );
            UpdateSkyboxMainTexture(renderTexture);
            Resources.UnloadUnusedAssets();
            screenMessage.Hide();
            yield return SkyboxFadeTransition(
                true, viewerSettings.fadeDuration
            );
            transitionLock = false;
            if (viewerSettings.autoPlay) StartCoroutine(ImageFadeOut());
        }
        firstRun = false;
    }

    void OnVideoEnd(VideoPlayer _)
    {
        if (viewerSettings.autoPlay)
        {
            StartCoroutine(ChangePanorama(PanoramaDirection.Next));
        }
    }

    void Start()
    {
        screenMessage = new(mainCamera);
        screenMessage.SetBackgroundColor(Color.black);

        // Scene blackout
        RenderSettings.skybox.SetFloat("_Exposure", 0);
        RenderSettings.skybox = null;

        // Load settings
        var settingsManager = new JsonSettingsManager(
            "PanoramaViewerSettings.json"
        );
        viewerSettings = settingsManager.Load(viewerSettings);

        // Set media files directory
        string mediaDir = Application.platform switch
        {
            RuntimePlatform.Android => Application.persistentDataPath,
            _ => Directory.GetParent(Application.dataPath).ToString()
        };
        mediaDir = Path.Combine(mediaDir, "PanoramaMediaFiles");

        // Check media files
        var fileFormats = viewerSettings.imageFormats
            .Concat(viewerSettings.videoFormats)
            .ToList();
        if (!Directory.Exists(mediaDir)) Directory.CreateDirectory(mediaDir);
        mediaFiles = GetFilesFromDir(mediaDir, fileFormats);
        if (mediaFiles.Count == 0)
        {
            screenMessage.SetText(
                "Media files not found\n\n"
                + $"Add files to\n\"{mediaDir}\"\n"
                + "and restart application"
            );
            return;
        }

        // Initialize Panoramic Skybox
        RenderSettings.skybox = new(Shader.Find("Skybox/Panoramic"));

        // Create Video Player
        var videoPlayerWrapper = new GameObject("Video Player");
        videoPlayer = videoPlayerWrapper.AddComponent<VideoPlayer>();
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.SetDirectAudioVolume(0, .5f);

        // ---

        var minimapWrapper = new GameObject("Minimap");
        var minimapCanvas = minimapWrapper.AddComponent<Canvas>();
        minimapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        minimapWrapper.AddComponent<CanvasScaler>();
        minimapWrapper.AddComponent<GraphicRaycaster>();

        var minimapImageWrapper = new GameObject("Minimap Image");
        minimapImageWrapper.transform.SetParent(minimapWrapper.transform);

        var minimapImage = minimapImageWrapper.AddComponent<RawImage>();
        var imagePath = @"";

        var imageData = File.ReadAllBytes(imagePath);
        minimapImage.texture = LoadTextureFromFile(imagePath);

        SetMinimapSizeAndPosition(minimapImage, MinimapPosition.TopRight, 1, 50f);

        // ---
        screenMessage.SetText("Loading ...");
        StartCoroutine(ChangePanorama(PanoramaDirection.Next));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (!transitionLock && !viewerSettings.autoPlay) ControlKeys();
    }
}
