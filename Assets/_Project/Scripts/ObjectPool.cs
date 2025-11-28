using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pooling system to prevent GC spikes on mobile
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool expandOnDemand = true;
    [SerializeField] private Transform poolContainer;

    private Queue<GameObject> _availableObjects = new Queue<GameObject>();
    private HashSet<GameObject> _activeObjects = new HashSet<GameObject>();

    private void Awake()
    {
        if (poolContainer == null)
            poolContainer = transform;

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledObject();
        }
    }

    private GameObject CreatePooledObject()
    {
        GameObject obj = Instantiate(prefab, poolContainer);
        obj.SetActive(false);
        _availableObjects.Enqueue(obj);
        return obj;
    }

    public GameObject Get()
    {
        GameObject obj;

        if (_availableObjects.Count > 0)
        {
            obj = _availableObjects.Dequeue();
        }
        else if (expandOnDemand)
        {
            obj = CreatePooledObject();
            _availableObjects.Dequeue();
        }
        else
        {
            Debug.LogWarning($"ObjectPool for {prefab.name} is empty and expandOnDemand is disabled");
            return null;
        }

        obj.SetActive(true);
        _activeObjects.Add(obj);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (!_activeObjects.Contains(obj))
        {
            Debug.LogWarning($"Attempting to return object {obj.name} that doesn't belong to this pool");
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(poolContainer);
        _activeObjects.Remove(obj);
        _availableObjects.Enqueue(obj);
    }

    public void ReturnAll()
    {
        var activeList = new List<GameObject>(_activeObjects);
        foreach (var obj in activeList)
        {
            Return(obj);
        }
    }

    public int ActiveCount => _activeObjects.Count;
    public int AvailableCount => _availableObjects.Count;
    public int TotalCount => ActiveCount + AvailableCount;
}