using UnityEngine;
using MoreMountains.Feedbacks;

// AttackCollision.cs for the player. Does damage to enemies and deflects enemy bullets.
public class AttackCollision : MonoBehaviour
{
    private PlayerController _controller; // player reference
    private int _damage; // player attack damage
    [SerializeField] private MMF_Player _feedback; // feedback for when the player hits an enemy
    [SerializeField] private MMF_Player _deflectFeedback; // feedback for when the player deflects an enemy bullet
    
    // Awake is called before the first frame update
    private void Awake()
    {
        _controller = GetComponentInParent<PlayerController>(); // get the player reference
        _damage = _controller.PlayerDamage; // get the player damage
    }

    /// <summary>
    /// This function detects collisions with specific layers and applies damage or deflects bullets
    /// accordingly.
    /// </summary>
    /// <param name="Collider">A Collider is a component that defines the shape of an object for the
    /// purposes of physical collisions. It can be attached to any GameObject and is used to detect
    /// collisions with other objects in the scene. When two Colliders intersect, a collision event is
    /// generated and can be detected by scripts attached to the GameObject</param>
    void OnTriggerEnter(Collider other) {
        // check if the other object is an enemy or damageable trap
        if (other != null && (other.gameObject.layer == LayerMask.NameToLayer("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("DamageableTrap")))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null) { damageable.TakeDamage(_damage); _feedback?.PlayFeedbacks(); }
        } 
        else if (other != null && other.gameObject.layer == LayerMask.NameToLayer("EnemyBullets")) // check if the other object is an enemy bullet
        {
            Bullet bullet = other.GetComponent<Bullet>(); // get the bullet component
            if (bullet != null) { 
                other.gameObject.layer = LayerMask.NameToLayer("PlayerBullet"); // change the bullet layer to player bullet
                bullet.Deflect(-other.transform.forward); _feedback?.PlayFeedbacks();  // deflect the bullet
            }
        }
    }
}
