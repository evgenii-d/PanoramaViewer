using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A utility class for displaying full-screen messages with a
/// customizable background and text, using a TextMeshPro component.
/// </summary>
public static class ScreenMessage
{
    private static GameObject canvasWrapper;
    private static TextMeshProUGUI textComponent;
    private static Image imageComponent;
    private static Camera renderCamera;

    /// <summary>
    /// Creates the UI canvas, background, and text components
    /// for displaying the screen message.
    /// </summary>
    /// <param name="camera">Main camera.</param>
    private static void CreateCanvas(Camera camera)
    {
        renderCamera = camera;
        canvasWrapper = new GameObject("Screen Message");
        canvasWrapper.SetActive(false);

        var canvas = canvasWrapper.AddComponent<Canvas>();
        canvasWrapper.AddComponent<CanvasScaler>();
        canvasWrapper.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = renderCamera;

        // Create the background for the message
        var background = new GameObject("Background");
        imageComponent = background.AddComponent<Image>();
        imageComponent.transform.SetParent(canvasWrapper.transform, false);
        imageComponent.rectTransform.sizeDelta = new Vector2(
            Screen.width, Screen.height
        ) * 1.5f;
        imageComponent.color = Color.black;

        // Create the text component for the message
        var textWrapper = new GameObject("Text Wrapper");
        textWrapper.transform.SetParent(canvasWrapper.transform, false);
        textComponent = textWrapper.AddComponent<TextMeshProUGUI>();
        textComponent.rectTransform.sizeDelta = new Vector2(
            Screen.width, Screen.height
        ) * 0.90f; // 90% of the screen size
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = true;
        textComponent.color = Color.white;
        textComponent.fontSize = Mathf.Min(Screen.width, Screen.height) / 30f;
    }

    /// <summary>
    /// Displays the specified message on the screen.
    /// </summary>
    /// <param name="camera">Main camera.</param>
    /// <param name="message">The message to display.</param>
    public static void Show(Camera camera, string message)
    {
        if (canvasWrapper == null || renderCamera != camera)
        {
            CreateCanvas(camera);
        }
        canvasWrapper.SetActive(true);
        textComponent.text = message;
    }

    /// <summary>
    /// Hides the screen message.
    /// </summary>
    public static void Hide()
    {
        if (canvasWrapper != null) canvasWrapper.SetActive(false);
    }
}