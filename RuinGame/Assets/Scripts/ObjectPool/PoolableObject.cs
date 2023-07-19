using UnityEngine;

// Poolable object class, which handles behaviour for objects that can be pooled
public class PoolableObject : MonoBehaviour
{
    public ObjectPool parent; // The pool that this object belongs to

    // Built-in Unity method, which is called when the object is disabled
    public virtual void OnDisable()
    {
        parent.ReturnObjectToPool(this); // Return the object to the pool
    }
}
