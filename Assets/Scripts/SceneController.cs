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
    ViewerConfig viewerConfig;
    VideoPlayer videoPlayer;
    // List to store the paths of images and videos to display
    List<string> mediaFiles;
    // Tracks the index of the currently displayed media file
    int currentMediaIndex = -1;
    // Prevents transitions from happening while one is in progress
    bool transitionLock = true;
    // Flag to indicate if this is the initial run
    bool firstRun = true;
    readonly List<string> imageFormats = new() { ".jpg", ".png" };
    readonly List<string> videoFormats = new() { ".mp4", ".webm" };

    public enum PanoramaDirection
    {
        Forward,
        Backward
    }

    public static List<string> GetFilesFromDir(
        string dirPath, IEnumerable<string> extensions = null
    )
    {
        if (extensions == null || !extensions.Any())
        {
            return Directory.GetFiles(dirPath).ToList();
        }

        extensions = extensions.Select(ext => ext.ToLowerInvariant());
        return Directory.EnumerateFiles(dirPath)
            .Where(
                file => extensions.Contains(
                    Path.GetExtension(file).ToLowerInvariant()
                )
            ).ToList();
    }

    /// <summary> Handles keyboard input for navigation </summary>
    void ControlKeys()
    {
        var keys = new Dictionary<KeyCode, PanoramaDirection>
        {
            { KeyCode.LeftArrow, PanoramaDirection.Backward },
            { KeyCode.PageDown, PanoramaDirection.Backward },
            { KeyCode.RightArrow, PanoramaDirection.Forward },
            { KeyCode.PageUp, PanoramaDirection.Forward }
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
            SkyboxFadeTransition(false, viewerConfig.fadeDuration)
        );
    }

    /// <summary> 
    /// Waits for the image display time before fading out
    /// </summary>
    IEnumerator ImageFadeOut()
    {
        yield return new WaitForSeconds(viewerConfig.imageDelay);
        yield return SkyboxFadeTransition(false, viewerConfig.fadeDuration);
        StartCoroutine(ChangePanorama(PanoramaDirection.Forward));
    }

    /// <summary>
    /// Fades in the skybox and unlocks transitions
    /// </summary>
    IEnumerator UnlockTransition()
    {
        StartCoroutine(
            SkyboxFadeTransition(true, viewerConfig.fadeDuration)
        );
        yield return new WaitForSeconds(viewerConfig.fadeDuration);
        transitionLock = false;
    }

    void OnVideoPrepared(VideoPlayer _)
    {
        var newRenderTexture = new RenderTexture(
            (int)videoPlayer.width, (int)videoPlayer.height, 32
        );

        if (viewerConfig.autoPlay)
        {
            var fadeDuration = viewerConfig.fadeDuration - 1;
            var timeBeforeEnd = (float)videoPlayer.length - fadeDuration;
            StartCoroutine(VideoFadeOut(timeBeforeEnd));
        }

        videoPlayer.targetTexture = newRenderTexture;
        ScreenMessage.Hide();
        UpdateSkyboxMainTexture(newRenderTexture);
        StartCoroutine(UnlockTransition());
        videoPlayer.Play();
    }

    IEnumerator ChangePanorama(PanoramaDirection direction)
    {
        transitionLock = true;
        currentMediaIndex += direction == PanoramaDirection.Forward ? 1 : -1;

        // Handles cycling back around if reaching end of the media file list
        currentMediaIndex = (
            currentMediaIndex + mediaFiles.Count
        ) % mediaFiles.Count;

        if (!viewerConfig.autoPlay && !firstRun)
        {
            yield return SkyboxFadeTransition(
                false, viewerConfig.fadeDuration
            );
        }

        string fileFormat = Path.GetExtension(mediaFiles[currentMediaIndex]);
        if (videoFormats.Contains(fileFormat))
        {
            videoPlayer.url = mediaFiles[currentMediaIndex];
            videoPlayer.Prepare();
        }
        else if (imageFormats.Contains(fileFormat))
        {
            videoPlayer.Stop();
            var renderTexture = ImageToRenderTexture(
                mediaFiles[currentMediaIndex]
            );
            UpdateSkyboxMainTexture(renderTexture);
            Resources.UnloadUnusedAssets();
            ScreenMessage.Hide();
            yield return SkyboxFadeTransition(
                true, viewerConfig.fadeDuration
            );
            transitionLock = false;
            if (viewerConfig.autoPlay) StartCoroutine(ImageFadeOut());
        }
        firstRun = false;
    }

    void OnVideoEnd(VideoPlayer _)
    {
        if (viewerConfig.autoPlay)
        {
            StartCoroutine(ChangePanorama(PanoramaDirection.Forward));
        }
    }

    void Start()
    {
        // Scene blackout
        RenderSettings.skybox.SetFloat("_Exposure", 0);
        RenderSettings.skybox = null;

        // Set app directories
        var appDataDir = Application.platform == RuntimePlatform.Android
            ? Application.persistentDataPath
            : Directory.GetParent(Application.dataPath).ToString();
        var mediaDir = Path.Combine(appDataDir, "PanoramaMediaFiles");
        var minimapsDir = Path.Combine(mediaDir, "Minimaps");

        Directory.CreateDirectory(mediaDir);
        Directory.CreateDirectory(minimapsDir);

        // Load settings
        var settingsManager = new JsonConfigManager(
            Path.Combine(appDataDir, "PanoramaViewerConfig.json")
        );
        viewerConfig = settingsManager.Load<ViewerConfig>();

        // Get media files
        mediaFiles = GetFilesFromDir(
            mediaDir, imageFormats.Union(videoFormats)
        );

        if (mediaFiles.Count == 0)
        {
            ScreenMessage.Show(
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
        var minimap = new GameObject("Minimap");
        var minimapCanvas = minimap.AddComponent<Canvas>();
        minimapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        minimap.AddComponent<CanvasScaler>();
        minimap.AddComponent<GraphicRaycaster>();

        var minimapImageWrapper = new GameObject("Minimap Image");
        minimapImageWrapper.transform.SetParent(minimap.transform);

        var minimapImage = minimapImageWrapper.AddComponent<RawImage>();
        var imagePath = @"";

        var imageData = File.ReadAllBytes(imagePath);
        minimapImage.texture = LoadTextureFromFile(imagePath);

        SetMinimapSizeAndPosition(minimap, MinimapPosition.TopRight, 1, 50f);
        // ---

        ScreenMessage.Show("Loading ...");
        StartCoroutine(ChangePanorama(PanoramaDirection.Forward));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (!transitionLock && !viewerConfig.autoPlay) ControlKeys();
    }
}
