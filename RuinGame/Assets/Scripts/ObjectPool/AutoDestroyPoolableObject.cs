using UnityEngine;

// Auto destroy poolable object class, which handles behaviour for objects that can be pooled and
// need to be destroyed after a certain amount of time, such as projectiles
public class AutoDestroyPoolableObject : PoolableObject
{
    [SerializeField] private float _autoDestroyTime = 5f; // The amount of time before the object is destroyed
    private const string DisableMethodName = "Disable"; // The name of the Disable method
    
    // Built-in Unity method, which is called when the object is enabled
    public virtual void OnEnable()
    {
        CancelInvoke(DisableMethodName); // Cancel any previous invocations of the Disable method
        Invoke(DisableMethodName, _autoDestroyTime); // Invoke the Disable method after the specified amount of time
    }

    /// <summary>
    /// This function disables the game object.
    /// </summary>
    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }
}
