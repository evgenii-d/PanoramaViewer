using System;
using System.IO;
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

    public override string ToString() { return JsonUtility.ToJson(this, true); }
}

public class FreeCamera : MonoBehaviour
{
    public FreeCameraSettings cameraSettings = new();
    Camera currentCamera;
    Vector2 rotation;

    void Start()
    {
        string settingsFile = "FreeCameraSettings.json";
        string appDir = Application.platform switch
        {
            RuntimePlatform.Android => Application.persistentDataPath,
            _ => Directory.GetParent(Application.dataPath).ToString()
        };

        currentCamera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        settingsFile = Path.Combine(appDir, settingsFile);

        if (!File.Exists(settingsFile)) File.WriteAllText(settingsFile, cameraSettings.ToString());
        cameraSettings = JsonUtility.FromJson<FreeCameraSettings>(File.ReadAllText(settingsFile));
    }

    void Update()
    {
        rotation.x += Input.GetAxis("Mouse X") * cameraSettings.mouseSensitivity;
        rotation.y += Input.GetAxis("Mouse Y") * cameraSettings.mouseSensitivity;
        transform.localRotation = Quaternion.Euler(-rotation.y, rotation.x, 0);

        // Detect right mouse click
        if (Input.GetMouseButton(1) && cameraSettings.zoom)
            currentCamera.fieldOfView = cameraSettings.fieldOfView / cameraSettings.zoomMagnification;
        else
            currentCamera.fieldOfView = cameraSettings.fieldOfView;
    }
}
