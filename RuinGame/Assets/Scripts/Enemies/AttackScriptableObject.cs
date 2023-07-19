using UnityEngine;

// Class for defining attack configurations, used to set up enemy attacks
[CreateAssetMenu(fileName = "Attack Configuration", menuName = "ScriptableObject/Attack Configuration")]
public class AttackScriptableObject : ScriptableObject
{
    [SerializeField] bool _isRanged = false; // Is the attack ranged?
    [SerializeField] float _attackRate = 1.5f; // Attack rate
    [SerializeField] float _attackRadius = 2f; // Attack radius

    // Ranged configs
    [SerializeField] Bullet _bulletPrefab; // Bullet prefab
    [SerializeField] Vector3 _bulletOffset = new Vector3(0, 1, 0); // Bullet offset

    /// <summary>
    /// This function sets up an enemy's attack radius and damage, and if the enemy is ranged, it also
    /// sets up its bullet prefab and creates a bullet pool.
    /// </summary>
    /// <param name="Enemy">An object representing an enemy in the game.</param>
    /// <param name="EnemyAbility">An object that contains information about the abilities of the enemy,
    /// such as damage dealt by attacks.</param>
    public void SetupEnemy(Enemy enemy, EnemyAbility enemyAbility)
    {
        enemy.AttackRadius.Radius_AttackRange = _attackRadius;
        enemy.AttackRadius.Radius_Damage = enemyAbility.EADamage;

        if (_isRanged)
        {
            RangedAttackRadius rangedAttackRadius = enemy.AttackRadius.GetComponent<RangedAttackRadius>();

            rangedAttackRadius.BulletPrefab = _bulletPrefab;
            rangedAttackRadius.BulletOffset = _bulletOffset;

            rangedAttackRadius.CreateBulletPool(); // Create a bullet pool
            
        }
    }
}
