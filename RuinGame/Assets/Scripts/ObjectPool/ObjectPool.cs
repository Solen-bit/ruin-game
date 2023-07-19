using System.Collections.Generic;
using UnityEngine;

// Object pool class, which handles behaviour for object pools
public class ObjectPool
{
    private GameObject _parent; // The parent of the objects in the pool
    private PoolableObject _prefab; // The prefab that this object pool returns instances of
    private int size; // The maximum number of objects that can be created
    private List<PoolableObject> _availableObjects; // A collection of currently inactive objects

    /* This is a private constructor for the ObjectPool class that takes in a PoolableObject prefab and
    an integer size as parameters. It initializes the _prefab and size fields of the ObjectPool
    instance with the values passed in as parameters, and creates a new List of PoolableObject
    instances with a capacity equal to the size parameter. */
    private ObjectPool(PoolableObject prefab, int size)
    {
        this._prefab = prefab;
        this.size = size;
        _availableObjects = new List<PoolableObject>(size);
    }

    /// <summary>
    /// This function creates an object pool instance with a specified size and populates it with a
    /// given prefab.
    /// </summary>
    /// <param name="prefab">The object that will be pooled.</param>
    /// <param name="size">The size parameter is an integer value that represents the initial size of
    /// the object pool. It determines how many instances of the PoolableObject prefab will be created
    /// and added to the pool when it is first created.</param>
    /// <returns>
    /// An instance of the ObjectPool class.
    /// </returns>
    public static ObjectPool CreateInstance(PoolableObject prefab, int size)
    {
        ObjectPool pool = new ObjectPool(prefab, size);

        pool._parent = new GameObject(prefab.name + " Pool");
        pool.PopulatePool();
        return pool;
    }

    /// <summary>
    /// The function creates a pool of objects by calling the CreateObject() function a specified number
    /// of times.
    /// </summary>
    private void PopulatePool()
    {
        for (int i = 0; i < size; i++) { CreateObject(); }
    }

    /// <summary>
    /// This function creates a new object from a prefab and sets it as inactive in a pool.
    /// </summary>
    private void CreateObject()
    {
        /* The Vector3.zero and Quaternion.identity parameters are used to set
        the position and rotation of the instantiated object to the default values (0,0,0) and no
        rotation, respectively. The _parent.transform parameter is used to set the parent of the
        instantiated object to the _parent GameObject, which is the parent object of all objects in
        the pool. */
        PoolableObject poolableObject = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity, _parent.transform);
        poolableObject.parent = this;
        poolableObject.gameObject.SetActive(false);
    }

    /// <summary>
    /// This function adds a poolable object back to the available objects list in a pool.
    /// </summary>
    public void ReturnObjectToPool(PoolableObject poolableObject)
    {
        _availableObjects.Add(poolableObject);
    }

    /// <summary>
    /// This function returns a poolable object and expands the pool if there are no available objects.
    /// </summary>
    /// <returns>
    /// The method is returning a PoolableObject instance.
    /// </returns>
    public PoolableObject GetObject()
    {
        if (_availableObjects.Count == 0) // expand pool if out of objects
        {
            CreateObject(); // create a new object
        }

        PoolableObject instance = _availableObjects[0]; // get the first available object
        _availableObjects.RemoveAt(0); // remove the object from the available objects list
        instance.gameObject.SetActive(true); // set the object as active
        return instance;
    }

    /// <summary>
    /// This function destroys the parent game object of the first available object in a pool.
    /// </summary>
    public void DestroyPool()
    {
        GameObject.Destroy(_availableObjects[0].transform.parent.gameObject);
    }
}
