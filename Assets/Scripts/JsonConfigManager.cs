using System.IO;
using UnityEngine;

/// <summary>
/// Manages saving and loading of configuration data in JSON format.
/// </summary>
public class JsonConfigManager
{
    readonly string configPath;

    /// <summary>
    /// Initializes the manager with the path to the config file.
    /// </summary>
    /// <param name="path">The file path for the configuration.</param>
    public JsonConfigManager(string path) { configPath = path; }

    /// <summary>
    /// Saves the given data to the config file in JSON format.
    /// </summary>
    /// <typeparam name="T">Type of the data to save.</typeparam>
    /// <param name="data">Data to save.</param>
    public void Save<T>(T data)
    {
        File.WriteAllText(configPath, JsonUtility.ToJson(data, true));
    }

    /// <summary>
    /// Loads the data from the config file.
    /// If the file does not exist, a default instance
    /// of the data is created, saved, and returned.
    /// </summary>
    /// <typeparam name="T">Type of data to load.</typeparam>
    /// <returns>
    /// Loaded data or default instance if file does not exist.
    /// </returns>
    public T Load<T>() where T : new()
    {
        if (File.Exists(configPath))
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(configPath));
        }

        var defaultData = new T();
        Save(defaultData);
        return defaultData;
    }
}