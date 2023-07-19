using UnityEngine;

// Path: Assets\Scripts\RangedAttackRadius.cs
// Class for the in-game ranged attack radius. Handles bullet pools and instantiating bullets.
public class RangedAttackRadius : AttackRadius
{
    [SerializeField] private Bullet _bulletPrefab; // Bullet prefab
    [SerializeField] private Vector3 _bulletOffset = new Vector3(0, 1, 0); // Offset for the bullet
    [SerializeField] private TurretEnemy _statEnemy; // Turret enemy. If this is not null, the bullet will be spawned at the turret enemy's position.
    
    private ObjectPool _bulletPool;
    private Bullet _bullet;

    // Getters and setters
    public Bullet BulletPrefab { get => _bulletPrefab; set => _bulletPrefab = value; }
    public Vector3 BulletOffset { get => _bulletOffset; set => _bulletOffset = value; }

    /// <summary>
    /// This function creates a bullet object pool if it doesn't already exist.
    /// </summary>
    public void CreateBulletPool()
    {
        if (_bulletPool == null)
        {
            _bulletPool = ObjectPool.CreateInstance(_bulletPrefab, Mathf.CeilToInt((1 / Radius_AttackRate) * _bulletPrefab.BulletAutoDestroyTime));
        }
    }

    /// <summary>
    /// This function spawns a bullet from a pool and sets its damage, position, and rotation before
    /// calling its Spawn method with a direction and target.
    /// </summary>
    public void AttackRadiusShoot()
    {
        WaitForSeconds wait = new WaitForSeconds(Radius_AttackRate);

        PoolableObject poolableObject = _bulletPool.GetObject();
        if (poolableObject != null)
        {
            _bullet = poolableObject.GetComponent<Bullet>();

            _bullet.BulletDamage = Radius_Damage;
            _bullet.transform.position = transform.position + _bulletOffset;
            _bullet.transform.rotation = transform.rotation;

            if (_statEnemy != null) { _bullet.Spawn(transform.forward, Radius_Damage, _statEnemy.GetTransform()); } // If the enemy is a turret, spawn the bullet at the turret's position
            else { _bullet.Spawn(transform.forward, Radius_Damage, _enemy.PlayerController.GetTransform()); } 
        }
    }
}
