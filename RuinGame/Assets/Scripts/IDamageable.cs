using UnityEngine;

// Defining an interface named `IDamageable`. The interface has
// two methods: `TakeDamage` and `GetTransform`. The `TakeDamage` method takes an integer
// parameter named `damage`. The `GetTransform` method returns a `Transform` object.
// All damageable objects must implement these two methods.
public interface IDamageable
{
    void TakeDamage(int damage);
    
    Transform GetTransform();
}
