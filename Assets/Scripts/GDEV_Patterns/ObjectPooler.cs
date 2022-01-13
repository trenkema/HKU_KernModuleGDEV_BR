using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] string _poolName;
    public string poolName { get { return _poolName; } }

    [SerializeField] GameObject _poolPrefab;
    public GameObject poolPrefab { get { return _poolPrefab; } }

    [SerializeField] int poolSize;
    [SerializeField] bool instantiateOnAwake;
    [SerializeField] bool autoResize;

    private bool instantiated = false;

    private List<GameObject> pooledObjects;

    private void Start()
    {
        pooledObjects = new List<GameObject>();

        if (instantiateOnAwake && !instantiated)
        {
            instantiated = true;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(poolPrefab);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }
    }

    public GameObject RequestPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }

        if (autoResize)
        {
            GameObject obj = Instantiate(poolPrefab);
            pooledObjects.Add(obj);
            return obj;
        }

        return null;
    }

    public void CleanPoolOverSize()
    {
        int count = pooledObjects.Count - 1;

        for (int i = count; i >= poolSize; i--)
        {
            GameObject pooledObject = pooledObjects[i];
            pooledObjects.Remove(pooledObject);
            Destroy(pooledObject);
        }
    }

    public void InstantiatePool()
    {
        if (!instantiateOnAwake && !instantiated)
        {
            instantiated = true;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(poolPrefab);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }
    }
}