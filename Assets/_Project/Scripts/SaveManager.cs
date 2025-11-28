using UnityEngine;
using System.IO;

/// <summary>
/// Data structure for save files.
/// </summary>
[System.Serializable]
public class SaveData
{
    public string currentSystemId;
    public float shipX;
    public float shipY;
    public float shipRotation;
}

/// <summary>
/// Singleton manager for saving and loading game state.
/// Uses Application.persistentDataPath for cross-platform compatibility.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Save game data to persistent storage.
    /// </summary>
    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to {SavePath}");
    }

    /// <summary>
    /// Load game data from persistent storage.
    /// </summary>
    public SaveData Load()
    {
        if (!File.Exists(SavePath)) return null;
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    /// <summary>
    /// Delete the save file.
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted");
        }
    }
}
