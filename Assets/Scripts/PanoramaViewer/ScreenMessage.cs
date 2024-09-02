using System;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("Make field readonly", "IDE0044")]
public class ScreenMessage
{
    TextMeshPro textComponent;
    GameObject canvasWrapper;
    Canvas uiCanvas;
    Image imageComponent;

    public void Show() { canvasWrapper.SetActive(true); }

    public void Hide() { canvasWrapper.SetActive(false); }

    public void SetText(string text) { textComponent.text = text; }

    public void SetBackgroundColor(Color color)
    {
        imageComponent.color = color;
    }

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
        var canvasSize = uiCanvas.GetComponent<RectTransform>().sizeDelta;
        var background = new GameObject("Background");
        imageComponent = background.AddComponent<Image>();
        var imageTransform = background.GetComponent<RectTransform>();

        imageComponent.transform.SetParent(
            canvasWrapper.transform, false
        );
        imageTransform.sizeDelta = new Vector2(
            canvasSize.x * 1.5f, canvasSize.y * 1.5f
        );
        imageTransform.anchoredPosition3D = new Vector3(0, 0, 1);
        imageComponent.color = Color.clear;

        // Message text
        var textWrapper = new GameObject("Text Wrapper");
        textWrapper.transform.SetParent(canvasWrapper.transform, false);
        textComponent = textWrapper.AddComponent<TextMeshPro>();
        textComponent.fontSharedMaterial.shader = Shader.Find(
            "TextMeshPro/Distance Field Overlay"
        );

        var textTransform = textWrapper.GetComponent<RectTransform>();
        textTransform.sizeDelta = new Vector2(
            canvasSize.x / 4, canvasSize.y
        );
        textComponent.fontSize = canvasSize.y * 20 / 100;
        textComponent.alignment = TextAlignmentOptions.Center;
    }
}