#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class StarSystemImporter : EditorWindow
{
    private string _jsonPath = "";
    
    [MenuItem("Milo/Import Star System JSON")]
    public static void ShowWindow()
    {
        GetWindow<StarSystemImporter>("Star System Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Star System from JSON", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Select JSON File"))
        {
            _jsonPath = EditorUtility.OpenFilePanel("Select Star System JSON", "", "json");
        }

        if (!string.IsNullOrEmpty(_jsonPath))
        {
            EditorGUILayout.LabelField("Selected:", _jsonPath);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Import to StreamingAssets"))
            {
                ImportSystem();
            }
        }
    }

    private void ImportSystem()
    {
        if (!File.Exists(_jsonPath))
        {
            EditorUtility.DisplayDialog("Error", "JSON file not found", "OK");
            return;
        }

        string sourceFolder = Path.GetDirectoryName(_jsonPath);
        string systemName = Path.GetFileNameWithoutExtension(_jsonPath).Replace("_system", "");
        
        string destFolder = Path.Combine(Application.streamingAssetsPath, "StarSystems", systemName);
        Directory.CreateDirectory(destFolder);

        string destJson = Path.Combine(destFolder, Path.GetFileName(_jsonPath));
        File.Copy(_jsonPath, destJson, overwrite: true);

        foreach (string pngFile in Directory.GetFiles(sourceFolder, "*.png"))
        {
            string destPng = Path.Combine(destFolder, Path.GetFileName(pngFile));
            File.Copy(pngFile, destPng, overwrite: true);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Imported {systemName} to StreamingAssets!", "OK");
    }
}
#endif