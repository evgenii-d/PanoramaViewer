using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Provides utility methods for working with files,
/// including file retrieval and filtering based on extensions.
/// </summary>
public static class FileExplorer
{
    /// <summary>
    /// Retrieves a list of files from the specified directory.
    /// Optionally filters the files based on provided extensions.
    /// </summary>
    /// <param name="dirPath">
    /// The directory path to search in.
    /// </param>
    /// <param name="extensions">
    /// Optional: A list of allowed file extensions.
    /// If null, all files are returned.
    /// </param>
    /// <returns>
    /// A list of file paths in the directory that match the given
    /// extensions or all files if no filter is provided.
    /// </returns>
    public static List<string> GetFilesFromDir(
        string dirPath,
        IEnumerable<string> extensions = null
    )
    {
        // Return all files if no extensions are provided
        if (extensions == null || !extensions.Any())
        {
            return Directory.GetFiles(dirPath).ToList();
        }

        // Filter files by their extensions
        extensions = extensions.Select(ext => ext.ToLowerInvariant());
        return Directory.EnumerateFiles(dirPath)
            .Where(
                file => extensions.Contains(
                    Path.GetExtension(file).ToLowerInvariant()
                )
            ).ToList();
    }

    /// <summary>
    /// Searches for a file by its name (without extension) in the
    /// specified directory. Optionally restricts the search to certain
    /// allowed file extensions.
    /// </summary>
    /// <param name="directoryPath">
    /// The path of the directory to search in.
    /// </param>
    /// <param name="fileNameWithoutExtension">
    /// The name of the file to search for, without the extension.
    /// </param>
    /// <param name="allowedExtensions">
    /// Optional: A list of allowed file extensions. If provided,
    /// only files with these extensions will be considered.
    /// </param>
    /// <returns>
    /// The file path if found, or null if the file does not exist or
    /// does not match the allowed extensions.
    /// </returns>
    public static string FindFile(
        string directoryPath,
        string fileNameWithoutExtension,
        List<string> allowedExtensions = null
    )
    {
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogWarning("Directory not found: " + directoryPath);
            return null;
        }

        // Search for files that match the given filename
        var files = Directory.EnumerateFiles(
            directoryPath,
            $"{fileNameWithoutExtension}.*",
            SearchOption.TopDirectoryOnly
        );

        // If allowedExtensions is provided, filter files by extension
        if (allowedExtensions != null && allowedExtensions.Count > 0)
        {
            files = files.Where(
                file => allowedExtensions.Contains(Path.GetExtension(file))
            );
        }

        // Return the first matching file or null
        return files.FirstOrDefault();
    }
}