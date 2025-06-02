using UnityEngine;
using System.IO;

#nullable enable
public static class SaveManager
{
    private const string SAVE_FILE_NAME = "save.json";

    /// <summary>
    /// Save the data to a JSON file.
    /// </summary>
    /// <param name="save">The gamedata save</param>
    public static void SaveGame(GameDataSave save)
    {
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(GetSavePath(), json);
    }

    /// <summary>
    /// Delete the save file if it exists.
    /// </summary>
    public static void DeleteSaveFile()
    {
        if (File.Exists(GetSavePath()))
        {
            File.Delete(GetSavePath());
        }
    }

    /// <summary>
    /// Check if the save file exists.
    /// </summary>
    /// <returns></returns>
    public static bool SaveExists()
    {
        return File.Exists(GetSavePath());
    }

    /// <summary>
    /// Load the game data from a JSON file.
    /// </summary>
    /// <returns>The saved data if it exists, null otherwise</returns>
    public static GameDataSave? LoadGame()
    {
        if (!SaveExists())
            return null;

        string json = File.ReadAllText(GetSavePath());
        GameDataSave save = JsonUtility.FromJson<GameDataSave>(json);

        return save;
    }

    /// <summary>
    /// Get the path where the save file is stored.
    /// </summary>
    /// <returns></returns>
    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "SAVE_FILE_NAME");
    }

}
