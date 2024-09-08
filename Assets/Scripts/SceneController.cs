using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Assets.Scripts.PanoramaViewer;
using static FileExplorer;
using static Assets.Scripts.PanoramaViewer.TextureLoader;
using static Assets.Scripts.PanoramaViewer.PanoramicSkyboxControl;

public class SceneController : MonoBehaviour
{
    public Camera mainCamera;
    ViewerConfig viewerConfig;
    VideoPlayer videoPlayer;
    Minimap minimap;

    // Directory where minimap images are stored
    string minimapsDir;

    // List to store the paths of images and videos to display
    List<string> mediaFiles;

    // Tracks the index of the currently displayed media file
    int currentMediaIndex = -1;

    // Prevents transitions from happening while one is in progress
    bool transitionLock = true;

    // Flag to indicate if this is the initial run
    bool firstRun = true;

    // Supported image and video formats
    readonly List<string> imageFormats = new() { ".jpg", ".png" };
    readonly List<string> videoFormats = new() { ".mp4", ".webm" };

    public enum PanoramaDirection { Forward, Backward }

    /// <summary>
    /// Handles keyboard input for navigating the media files.
    /// </summary>
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
    /// Fades out the minimap if it is currently visible.
    /// </summary>
    void MinimapFadeOut()
    {
        if (minimap.IsVisible())
        {
            StartCoroutine(
                minimap.FadeTransition(false, viewerConfig.fadeDuration)
            );
        }
    }

    /// <summary>
    /// Coroutine that waits for a delay 
    /// before fading out the video and minimap.
    /// </summary>
    IEnumerator VideoFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(
            SkyboxFadeTransition(false, viewerConfig.fadeDuration)
        );
        MinimapFadeOut();
    }

    /// <summary>
    /// Coroutine that waits for the image display time 
    /// before fading out and moving to the next panorama.
    /// </summary>
    IEnumerator ImageFadeOut()
    {
        yield return new WaitForSeconds(viewerConfig.imageDisplayTime);
        MinimapFadeOut();
        yield return SkyboxFadeTransition(false, viewerConfig.fadeDuration);
        StartCoroutine(ChangePanorama(PanoramaDirection.Forward));
    }

    /// <summary>
    /// Coroutine that unlocks transitions 
    /// after the skybox fade-in animation is complete.
    /// </summary>
    IEnumerator UnlockTransition()
    {
        StartCoroutine(
            SkyboxFadeTransition(true, viewerConfig.fadeDuration)
        );
        yield return new WaitForSeconds(viewerConfig.fadeDuration);
        transitionLock = false;
    }

    /// <summary>
    /// Callback function for when a video is prepared.
    /// Sets the render texture and starts video playback.
    /// </summary>
    void OnVideoPrepared(VideoPlayer _)
    {
        var newRenderTexture = new RenderTexture(
            (int)videoPlayer.width, (int)videoPlayer.height, 32
        );

        if (viewerConfig.autoPlay)
        {
            var timeBeforeEnd = (float)videoPlayer.length - viewerConfig.fadeDuration;
            StartCoroutine(VideoFadeOut(timeBeforeEnd));
        }

        videoPlayer.targetTexture = newRenderTexture;
        ScreenMessage.Hide();
        UpdateSkyboxMainTexture(newRenderTexture);
        StartCoroutine(UnlockTransition());
        videoPlayer.Play();
    }

    /// <summary>
    /// Changes the current panorama image or video 
    /// based on the specified direction.
    /// Handles transitions and updating the minimap.
    /// </summary>
    IEnumerator ChangePanorama(PanoramaDirection direction)
    {
        transitionLock = true;
        currentMediaIndex += direction == PanoramaDirection.Forward ? 1 : -1;

        // Handles cycling back around if reaching end of the media file list
        currentMediaIndex = (currentMediaIndex + mediaFiles.Count)
            % mediaFiles.Count;

        var filePath = mediaFiles[currentMediaIndex];
        var fileFormat = Path.GetExtension(filePath);
        var minimapImage = FindFile(
            minimapsDir,
            Path.GetFileNameWithoutExtension(filePath),
            imageFormats
        );

        MinimapFadeOut();
        if (!viewerConfig.autoPlay && !firstRun)
        {
            yield return SkyboxFadeTransition(
                false, viewerConfig.fadeDuration
            );
        }

        if (minimapImage != null)
        {
            minimap.SetImage(minimapImage);
            StartCoroutine(
                minimap.FadeTransition(true, viewerConfig.fadeDuration)
            );
        }

        ScreenMessage.Hide();
        if (videoFormats.Contains(fileFormat))
        {
            videoPlayer.url = filePath;
            videoPlayer.Prepare();
        }
        else if (imageFormats.Contains(fileFormat))
        {
            videoPlayer.Stop();
            var renderTexture = ImageToRenderTexture(filePath);
            UpdateSkyboxMainTexture(renderTexture);
            Resources.UnloadUnusedAssets();
            yield return SkyboxFadeTransition(
                true, viewerConfig.fadeDuration
            );
            if (viewerConfig.autoPlay) StartCoroutine(ImageFadeOut());
        }
        firstRun = false;
        transitionLock = false;
    }

    /// <summary>
    /// Callback function for when a video ends. 
    /// Moves to the next panorama if autoPlay is enabled.
    /// </summary>
    void OnVideoEnd(VideoPlayer _)
    {
        if (viewerConfig.autoPlay)
        {
            StartCoroutine(ChangePanorama(PanoramaDirection.Forward));
        }
    }

    void Start()
    {
        // Initialize Panoramic Skybox
        RenderSettings.skybox.SetFloat("_Exposure", 0);
        RenderSettings.skybox = new(Shader.Find("Skybox/Panoramic"));

        // Set app directories
        var appDataDir = Application.platform == RuntimePlatform.Android
            ? Application.persistentDataPath
            : Directory.GetParent(Application.dataPath).ToString();
        var mediaDir = Path.Combine(appDataDir, "PanoramaMediaFiles");
        minimapsDir = Path.Combine(mediaDir, "Minimaps");

        Directory.CreateDirectory(mediaDir);
        Directory.CreateDirectory(minimapsDir);

        // Load viewer configuration from JSON
        var settingsManager = new JsonConfigManager(
            Path.Combine(appDataDir, "PanoramaViewerConfig.json")
        );
        viewerConfig = settingsManager.Load<ViewerConfig>();

        // Get list of media files
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

        // Create Video Player
        var videoPlayerWrapper = new GameObject("Video Player");
        videoPlayer = videoPlayerWrapper.AddComponent<VideoPlayer>();
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.SetDirectAudioVolume(0, 0.5f);

        // Initialize minimap
        minimap = new Minimap();
        minimap.Scale(viewerConfig.minimap.scale);
        minimap.SetPosition(
            viewerConfig.minimap.position,
            viewerConfig.minimap.offset
        );

        ScreenMessage.Show("Loading ...");
        StartCoroutine(ChangePanorama(PanoramaDirection.Forward));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (!transitionLock && !viewerConfig.autoPlay) ControlKeys();
    }
}
