using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)] [SerializeField] float distanceToPlayer = 10f;
    [Range(0, 90)] [SerializeField] float angleToPlayer = 75f;


    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.5f;  // Duration of the shake effect
    public float shakeMagnitude = 0.5f; // Magnitude of the shake effect
    public float dampingSpeed = 1.0f;   // Speed at which the shake effect fades away


    GameObject player;
    Transform initialTransform;
    float initialZOffset;

    private Vector3 initialPosition;    // Initial position of the camera for shake
    private float currentShakeDuration; // Remaining shake duration
    private bool isShaking = false;     // Check if the camera is shaking


    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.player;
        Quaternion newRotation = Quaternion.identity;
        newRotation = Quaternion.Euler(newRotation.x + angleToPlayer, newRotation.y, newRotation.z);
        transform.SetPositionAndRotation(player.transform.position, newRotation);
        transform.position -= transform.TransformDirection(Vector3.forward) * distanceToPlayer;
        
        initialTransform = transform;
        initialZOffset = transform.position.z - player.transform.position.z;
        initialPosition = transform.localPosition; // Save the initial camera position for shaking
    }

    // Update is called once per frame
    void Update()
    {
        if (!isShaking) // Only follow the player when not shaking
        {
            FollowPlayer();
        }

        if (currentShakeDuration > 0)
        {
            // Apply camera shake using Random.insideUnitSphere for random shaking
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;

            // Decrease the remaining shake duration over time
            currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else if (isShaking)
        {
            // Reset the camera position after the shake ends
            transform.localPosition = initialPosition;
            isShaking = false;
        }
    }

    void FollowPlayer()
    {
        transform.SetPositionAndRotation(new Vector3(player.transform.position.x,
            transform.position.y,
            player.transform.position.z + initialZOffset),
            initialTransform.rotation);
    }

    // Public method to trigger the camera shake
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        currentShakeDuration = shakeDuration;
        isShaking = true;
    }
}
