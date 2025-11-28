using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MilosAdventure.Data;
using Newtonsoft.Json;

/// <summary>
/// Singleton loader for star system JSON and sprite assets.
/// Uses UnityWebRequest for Android compatibility.
/// </summary>
public class StarSystemLoader : MonoBehaviour
{
    public static StarSystemLoader Instance { get; private set; }

    [Header("Auto-Load")]
    [SerializeField] private string systemToLoad = "sol/sol";
    [SerializeField] private bool loadOnStart = true;

    private Dictionary<string, StarSystemJson> _loadedSystems = new();
    private Dictionary<string, Sprite> _loadedSprites = new();

    public System.Action<StarSystemJson> OnSystemLoaded;
    public System.Action<string> OnLoadFailed;

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

    private void Start()
    {
        if (loadOnStart && !string.IsNullOrEmpty(systemToLoad))
        {
            Debug.Log($"[StarSystemLoader] Auto-loading system: {systemToLoad}");
            LoadSystemAsync(systemToLoad);
        }
        else
        {
            Debug.Log($"[StarSystemLoader] Auto-load disabled or no system specified");
        }
    }

    /// <summary>
    /// Load a star system from StreamingAssets (async for Android compatibility).
    /// </summary>
    public void LoadSystemAsync(string systemId)
    {
        StartCoroutine(LoadSystemCoroutine(systemId));
    }

    private IEnumerator LoadSystemCoroutine(string systemId)
    {
        Debug.Log($"[StarSystemLoader] LoadSystemCoroutine started for: {systemId}");

        if (_loadedSystems.TryGetValue(systemId, out var cached))
        {
            Debug.Log($"[StarSystemLoader] Using cached system: {systemId}");
            OnSystemLoaded?.Invoke(cached);
            yield break;
        }

        string jsonPath = $"{Application.streamingAssetsPath}/StarSystems/{systemId}.json";

        // Add file:// prefix for local files in editor
        if (!jsonPath.StartsWith("http"))
        {
            jsonPath = "file://" + jsonPath;
        }

        Debug.Log($"[StarSystemLoader] Loading from path: {jsonPath}");

        using (UnityWebRequest request = UnityWebRequest.Get(jsonPath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load system {systemId}: {request.error}");
                OnLoadFailed?.Invoke(systemId);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log($"[StarSystemLoader] JSON loaded, length: {json.Length} characters");

            StarSystemJson system = JsonConvert.DeserializeObject<StarSystemJson>(json);

            if (system == null || system.rootBodies == null)
            {
                Debug.LogError($"Invalid JSON for system: {systemId}");
                OnLoadFailed?.Invoke(systemId);
                yield break;
            }

            system.FlattenHierarchy();
            Debug.Log($"[StarSystemLoader] JSON parsed successfully, {system.bodies.Count} bodies found (flattened from hierarchy)");

            yield return LoadBodiesSprites(system.bodies, systemId);

            _loadedSystems[systemId] = system;
            Debug.Log($"[StarSystemLoader] Invoking OnSystemLoaded event");
            OnSystemLoaded?.Invoke(system);
        }
    }

    private IEnumerator LoadBodiesSprites(List<CelestialBodyJson> bodies, string systemId)
    {
        foreach (var body in bodies)
        {
            if (!string.IsNullOrEmpty(body.sprite))
            {
                yield return LoadSpriteAsync(systemId, body.sprite);
            }
            yield return null;
        }
    }

    private IEnumerator LoadSpriteAsync(string systemId, string filename)
    {
        if (_loadedSprites.ContainsKey(filename))
            yield break;

        string path = $"{Application.streamingAssetsPath}/StarSystems/{systemId}/{filename}";

        // Add file:// prefix for local files in editor
        if (!path.StartsWith("http"))
        {
            path = "file://" + path;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                tex.filterMode = FilterMode.Bilinear;

                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                _loadedSprites[filename] = sprite;
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite {filename}: {request.error}");
            }
        }
    }

    public Sprite GetSprite(string filename)
    {
        _loadedSprites.TryGetValue(filename, out var sprite);
        return sprite;
    }

    public StarSystemJson GetCachedSystem(string systemId)
    {
        _loadedSystems.TryGetValue(systemId, out var system);
        return system;
    }
}
