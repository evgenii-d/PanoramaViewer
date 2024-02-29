using System;
using UnityEngine;

[Serializable]
public class FreeCameraSettings
{
    [Range(0.0f, 180.0f)]
    public float fieldOfView = 60f;
    [Range(0.0f, 10.0f)]
    public float mouseSensitivity = 1f;
    public bool zoom = true;
    [Range(1f, 10.0f)]
    public float zoomMagnification = 1.5f;
}

public class FreeCamera : MonoBehaviour
{
    public FreeCameraSettings cameraSettings = new();
    Camera currentCamera;
    Vector2 rotation;

    void Start()
    {
        JsonSettingsManager settingsManager = new("FreeCameraSettings.json");
        cameraSettings = settingsManager.Load(cameraSettings);
        currentCamera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        rotation.x += Input.GetAxis("Mouse X") * cameraSettings.mouseSensitivity;
        rotation.y += Input.GetAxis("Mouse Y") * cameraSettings.mouseSensitivity;
        currentCamera.transform.localRotation = Quaternion.Euler(-rotation.y, rotation.x, 0);

        // Detect right mouse click
        if (Input.GetMouseButton(1) && cameraSettings.zoom)
            currentCamera.fieldOfView = cameraSettings.fieldOfView / cameraSettings.zoomMagnification;
        else
            currentCamera.fieldOfView = cameraSettings.fieldOfView;
    }
}
