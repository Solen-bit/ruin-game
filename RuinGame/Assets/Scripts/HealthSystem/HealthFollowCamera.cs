using UnityEngine;

// Health follow camera class, which makes the health UI follow the camera
public class HealthFollowCamera : MonoBehaviour
{
    [SerializeField] Camera _camera; // Camera to follow

    // Getter and setter for the camera
    public Camera HealthFollow { get => _camera; set => _camera = value; }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + _camera.transform.forward); // Make the health UI follow the camera
    }
}
