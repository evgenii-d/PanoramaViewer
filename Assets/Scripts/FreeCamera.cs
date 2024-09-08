using System;
using System.IO;
using UnityEngine;

[Serializable]
public class CameraZoomConfig
{
    public bool enabled = true;
    [Range(1f, 10.0f)]
    public float magnification = 1.5f;
}

[Serializable]
public class FreeCameraConfig
{
    [Range(0.0f, 180.0f)]
    public float fieldOfView = 60f;
    [Range(0.0f, 10.0f)]
    public float mouseSensitivity = 1f;
    public CameraZoomConfig zoom = new();
}

public class FreeCamera : MonoBehaviour
{
    public FreeCameraConfig cameraConfig;
    Camera currentCamera;
    Vector2 rotation;

    void Start()
    {
        var appDataDir = Application.platform == RuntimePlatform.Android
            ? Application.persistentDataPath
            : Directory.GetParent(Application.dataPath).ToString();
        var settingsManager = new JsonConfigManager(
            Path.Combine(appDataDir, "FreeCameraConfig.json")
        );
        cameraConfig = settingsManager.Load<FreeCameraConfig>();
        currentCamera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        rotation.x +=
            Input.GetAxis("Mouse X") * cameraConfig.mouseSensitivity;
        rotation.y +=
            Input.GetAxis("Mouse Y") * cameraConfig.mouseSensitivity;
        currentCamera.transform.localRotation =
            Quaternion.Euler(-rotation.y, rotation.x, 0);

        // Detect right mouse click
        if (Input.GetMouseButton(1) && cameraConfig.zoom.enabled)
        {
            currentCamera.fieldOfView =
                cameraConfig.fieldOfView / cameraConfig.zoom.magnification;
        }
        else currentCamera.fieldOfView = cameraConfig.fieldOfView;
    }
}
