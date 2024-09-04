using System.IO;
using UnityEngine;

public class JsonConfigManager
{
    readonly string configPath;

    public JsonConfigManager(string path)
    {
        configPath = path;
    }

    public void Save<T>(T data)
    {
        File.WriteAllText(configPath, JsonUtility.ToJson(data, true));
    }

    public T Load<T>() where T : new()
    {
        if (File.Exists(configPath))
        {
            var data = File.ReadAllText(configPath);
            return JsonUtility.FromJson<T>(data);
        }
        var defaultData = new T();
        Save(defaultData);
        return defaultData;
    }
}