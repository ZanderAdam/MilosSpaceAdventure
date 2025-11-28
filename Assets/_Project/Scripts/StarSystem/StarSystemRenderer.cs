using System.Collections.Generic;
using UnityEngine;
using MilosAdventure.Data;

/// <summary>
/// Renders a star system by instantiating celestial body prefabs.
/// </summary>
public class StarSystemRenderer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject celestialBodyPrefab;

    [Header("Settings")]
    [SerializeField] private float scaleMultiplier = 15f;
    [SerializeField] private float orbitSpeedMultiplier = 1f;

    public float ScaleMultiplier => scaleMultiplier;

    private StarSystemJson _currentSystem;
    private Dictionary<string, CelestialBodyController> _bodies = new();

    private void Start()
    {
        if (StarSystemLoader.Instance != null)
        {
            Debug.Log("[StarSystemRenderer] Subscribing to OnSystemLoaded event");
            StarSystemLoader.Instance.OnSystemLoaded += RenderSystem;
        }
        else
        {
            Debug.LogError("[StarSystemRenderer] StarSystemLoader.Instance is null! Cannot subscribe to OnSystemLoaded");
        }
    }

    private void OnDisable()
    {
        if (StarSystemLoader.Instance != null)
        {
            StarSystemLoader.Instance.OnSystemLoaded -= RenderSystem;
        }
    }

    /// <summary>
    /// Render a complete star system from JSON data.
    /// </summary>
    public void RenderSystem(StarSystemJson system)
    {
        Debug.Log($"[StarSystemRenderer] RenderSystem called with {system.bodies.Count} bodies");
        ClearSystem();
        _currentSystem = system;

        foreach (var bodyData in system.bodies)
        {
            CreateBody(bodyData);
        }

        Debug.Log($"[StarSystemRenderer] Finished creating {_bodies.Count} celestial bodies");
    }

    private void CreateBody(CelestialBodyJson data)
    {
        Debug.Log($"[StarSystemRenderer] Creating body: {data.name} (id: {data.id})");
        GameObject go = Instantiate(celestialBodyPrefab, transform);
        go.name = data.name;

        var controller = go.GetComponent<CelestialBodyController>();
        controller.Initialize(data, this);

        _bodies[data.id] = controller;
    }

    public CelestialBodyController GetBody(string id)
    {
        _bodies.TryGetValue(id, out var body);
        return body;
    }

    public Vector3 GetParentPosition(string parentId)
    {
        if (string.IsNullOrEmpty(parentId)) return Vector3.zero;
        var parent = GetBody(parentId);
        return parent != null ? parent.transform.position : Vector3.zero;
    }

    private void ClearSystem()
    {
        foreach (var body in _bodies.Values)
        {
            if (body != null)
                Destroy(body.gameObject);
        }
        _bodies.Clear();
    }
}
