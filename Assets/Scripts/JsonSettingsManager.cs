using System.IO;
using UnityEngine;

public class JsonSettingsManager
{
    readonly string settingsPath;

    public JsonSettingsManager(string fullFilename)
    {
        string settingsDir = Application.platform switch
        {
            RuntimePlatform.Android => Application.persistentDataPath,
            _ => Directory.GetParent(Application.dataPath).ToString()
        };
        settingsPath = Path.Combine(settingsDir, fullFilename);
    }

    public void Save<T>(T jsonData)
    {
        File.WriteAllText(settingsPath, JsonUtility.ToJson(jsonData, true));
    }

    public T Load<T>(T jsonData)
    {
        if (File.Exists(settingsPath)) return JsonUtility.FromJson<T>(File.ReadAllText(settingsPath));
        Save(jsonData);
        return jsonData;
    }
}