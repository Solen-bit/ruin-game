using UnityEngine;
using MoreMountains.Feedbacks;

// Path: Assets\Scripts\BulletHitFX.cs
// Class for the in-game projectile effects. It is separate from the Bullet class because the effects
// need to persist after the bullet is destroyed.
public class BulletHitFX : MonoBehaviour
{
    [SerializeField] private MMF_Player _hitFeedback; // Bullet hit feedback
    [SerializeField] private float offsetX, offsetY, offsetZ = 0f; // Offset for the bullet hit feedback
    
    /// <summary>
    /// This function plays a feedback animation at a given position with an offset.
    /// </summary>
    /// <param name="Transform">Transform is a class in Unity that represents the position, rotation,
    /// and scale of a game object. It contains information about the object's position in 3D space, as
    /// well as its orientation and size. In this code snippet, the Transform parameter is used to
    /// specify the position of the bullet hit fx.</param>
    public void PlayBulletHitFeedback(Transform position)
    {
        Vector3 offsetPosition = new Vector3(position.position.x + offsetX, position.position.y + offsetY, position.position.z + offsetZ);
        gameObject.transform.position = offsetPosition;
        _hitFeedback?.PlayFeedbacks();
    }
}
