using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class ScreenMessage
{
    private static GameObject canvasWrapper;
    private static TextMeshProUGUI textComponent;
    private static Image imageComponent;

    static ScreenMessage()
    {
        CreateCanvas();
    }

    private static void CreateCanvas()
    {
        canvasWrapper = new GameObject("Screen Message");
        canvasWrapper.SetActive(false);

        var canvas = canvasWrapper.AddComponent<Canvas>();
        canvasWrapper.AddComponent<CanvasScaler>();
        canvasWrapper.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        // Text background
        var background = new GameObject("Background");
        imageComponent = background.AddComponent<Image>();
        imageComponent.transform.SetParent(canvasWrapper.transform, false);
        imageComponent.rectTransform.sizeDelta = new Vector2(
            Screen.width, Screen.height
        );
        imageComponent.color = Color.black;

        // Message text
        var textWrapper = new GameObject("Text Wrapper");
        textWrapper.transform.SetParent(canvasWrapper.transform, false);
        textComponent = textWrapper.AddComponent<TextMeshProUGUI>();
        textComponent.rectTransform.sizeDelta = new Vector2(
            Screen.width, Screen.height
        ) * 0.90f;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = true;
        textComponent.color = Color.white;
        textComponent.fontSize = Mathf.Min(Screen.width, Screen.height) / 30f;
    }

    public static void Show(string message)
    {
        if (canvasWrapper == null) CreateCanvas();
        canvasWrapper.SetActive(true);
        textComponent.text = message;
    }

    public static void Hide()
    {
        if (canvasWrapper != null) canvasWrapper.SetActive(false);
    }
}