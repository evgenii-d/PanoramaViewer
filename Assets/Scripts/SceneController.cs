using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using PanoramaViewer;
using static PanoramaViewer.FileOperations;
using static PanoramaViewer.PanoramicSkyboxControl;

public class SceneController : MonoBehaviour
{
    ViewerSettings viewerSettings = new();
    VideoPlayer videoPlayer;
    List<string> mediaFiles;
    int currentMediaIndex = -1;
    string appMessage = null;
    bool transitionLock = true;
    bool firstRun = true;

    void MessageWindow(string text)
    {
        Rect boxPosition = new(0, 0, Screen.width, Screen.height);
        int screenFraction = Screen.height / 30;
        GUIStyle textStyle = new(GUI.skin.box)
        {
            wordWrap = true,
            fontSize = screenFraction,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(screenFraction, screenFraction, screenFraction, screenFraction)
        };
        GUI.TextArea(boxPosition, text, textStyle);
    }

    void ControlKeys()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.PageDown))
        {
            StartCoroutine(ChangePanorama("prev"));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.PageUp))
        {
            StartCoroutine(ChangePanorama("next"));
        }
    }

    IEnumerator VideoFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(SkyboxFadeTransition("fadeOut", viewerSettings.fadeDuration));
    }

    IEnumerator ImageFadeOut()
    {
        yield return new WaitForSeconds(viewerSettings.imageDelay);
        yield return SkyboxFadeTransition("fadeOut", viewerSettings.fadeDuration);
        StartCoroutine(ChangePanorama("next"));
    }

    IEnumerator UnlockTransition()
    {
        StartCoroutine(SkyboxFadeTransition("fadeIn", viewerSettings.fadeDuration));
        yield return new WaitForSeconds(viewerSettings.fadeDuration);
        transitionLock = false;
    }

    void OnVideoPrepared(VideoPlayer _)
    {
        RenderTexture newRenderTexture = new((int)videoPlayer.width, (int)videoPlayer.height, 32);
        float timeBeforeEnd = (float)videoPlayer.length - viewerSettings.fadeDuration - 1;

        if (viewerSettings.autoPlay) StartCoroutine(VideoFadeOut(timeBeforeEnd));

        videoPlayer.targetTexture = newRenderTexture;
        appMessage = null;
        UpdateSkyboxMainTexture(newRenderTexture);
        StartCoroutine(UnlockTransition());
        videoPlayer.Play();
    }

    IEnumerator ChangePanorama(string direction)
    {
        transitionLock = true;
        currentMediaIndex = direction.ToLower() == "next" ? ++currentMediaIndex : --currentMediaIndex;

        if (currentMediaIndex > mediaFiles.Count - 1) currentMediaIndex = 0;
        if (currentMediaIndex < 0) currentMediaIndex = mediaFiles.Count - 1;

        if (!viewerSettings.autoPlay && !firstRun)
            yield return SkyboxFadeTransition("fadeOut", viewerSettings.fadeDuration);

        string fileFormat = Path.GetExtension(mediaFiles[currentMediaIndex]);
        switch (fileFormat)
        {
            case string _ when viewerSettings.videoFormats.Contains(fileFormat):
                videoPlayer.url = mediaFiles[currentMediaIndex];
                videoPlayer.Prepare();
                break;
            case string _ when viewerSettings.imageFormats.Contains(fileFormat):
                videoPlayer.Stop();
                RenderTexture renderTexture = ImageToRenderTexture(mediaFiles[currentMediaIndex]);
                UpdateSkyboxMainTexture(renderTexture);
                Resources.UnloadUnusedAssets();
                appMessage = null;
                yield return SkyboxFadeTransition("fadeIn", viewerSettings.fadeDuration);
                transitionLock = false;
                if (viewerSettings.autoPlay) StartCoroutine(ImageFadeOut());
                break;
            default:
                break;
        }
        firstRun = false;
    }

    void OnVideoEnd(VideoPlayer _) { if (viewerSettings.autoPlay) StartCoroutine(ChangePanorama("next")); }
    void OnGUI() { if (appMessage != null) MessageWindow(appMessage); }

    void Start()
    {
        // Scene blackout
        RenderSettings.skybox.SetFloat("_Exposure", 0);
        RenderSettings.skybox = null;

        JsonSettingsManager settingsManager = new("PanoramaViewerSettings.json");
        viewerSettings = settingsManager.Load(viewerSettings);

        // Set media files directory
        string mediaDir = Application.platform switch
        {
            RuntimePlatform.Android => Application.persistentDataPath,
            _ => Directory.GetParent(Application.dataPath).ToString()
        };
        mediaDir = Path.Combine(mediaDir, "PanoramaMediaFiles");

        // Check media files
        List<string> fileFormats = viewerSettings.imageFormats.Concat(viewerSettings.videoFormats).ToList();
        if (!Directory.Exists(mediaDir)) Directory.CreateDirectory(mediaDir);
        mediaFiles = GetFilesFromDir(mediaDir, fileFormats);
        if (mediaFiles.Count == 0)
        {
            appMessage = $"Media files not found\n\nAdd files to\n\"{mediaDir}\"\nand restart application";
            return;
        }

        // Initialize Panoramic Skybox
        RenderSettings.skybox = new(Shader.Find("Skybox/Panoramic"));

        // Initialize Video Player
        GameObject videoPlayerWrapper = new("Video Player Wrapper");
        videoPlayer = videoPlayerWrapper.AddComponent<VideoPlayer>();
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.SetDirectAudioVolume(0, .5f);

        appMessage = "Loading ...";
        StartCoroutine(ChangePanorama("next"));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (!transitionLock && !viewerSettings.autoPlay) ControlKeys();
    }
}
