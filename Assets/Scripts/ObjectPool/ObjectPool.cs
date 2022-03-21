using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private GameObject Parent;
    private PoolableObject Prefab;
    private int Size;
    private List<PoolableObject> AvailableObjectsPool;
    private static Dictionary<PoolableObject, ObjectPool> ObjectPools = new Dictionary<PoolableObject, ObjectPool>();

    private ObjectPool(PoolableObject Prefab, int Size)
    {
        this.Prefab = Prefab;
        this.Size = Size;
        AvailableObjectsPool = new List<PoolableObject>(Size);
    }

    public static ObjectPool CreateInstance(PoolableObject Prefab, int Size)
    {
        ObjectPool pool = null;

        if (ObjectPools.ContainsKey(Prefab))
        {
            pool = ObjectPools[Prefab];
        }
        else
        {
            pool = new ObjectPool(Prefab, Size);

            pool.Parent = new GameObject(Prefab + " Pool");
            pool.CreateObjects();

            ObjectPools.Add(Prefab, pool);
        }


        return pool;
    }

    private void CreateObjects()
    {
        for (int i = 0; i < Size; i++)
        {
            CreateObject();
        }
    }

    private void CreateObject()
    {
        PoolableObject poolableObject = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity, Parent.transform);
        poolableObject.Parent = this;
        poolableObject.gameObject.SetActive(false); // PoolableObject handles re-adding the object to the AvailableObjects
    }

    public PoolableObject GetObject(Vector3 Position, Quaternion Rotation)
    {
        if (AvailableObjectsPool.Count == 0) // auto expand pool size if out of objects
        {
            CreateObject();
        }

        PoolableObject instance = AvailableObjectsPool[0];

        AvailableObjectsPool.RemoveAt(0);

        instance.transform.position = Position;
        instance.transform.rotation = Rotation;

        instance.gameObject.SetActive(true);

        return instance;
    }

    public PoolableObject GetObject()
    {
        return GetObject(Vector3.zero, Quaternion.identity);
    }

    public void ReturnObjectToPool(PoolableObject Object)
    {
        AvailableObjectsPool.Add(Object);
    }
}