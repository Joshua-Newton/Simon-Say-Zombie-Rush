using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)][SerializeField] float distanceToPlayer = 10f;
    [Range(0, 90)][SerializeField] float angleToPlayer = 75f;

    [Header("Dead Zone Settings")]
    [SerializeField] private Vector2 deadZoneSize = new Vector2(2f, 2f);

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 5f;    // Minimum zoom distance
    [SerializeField] private float maxZoom = 20f;   // Maximum zoom distance
    [SerializeField] private float zoomSensitivity = 2f; // How sensitive the zoom control is
    private float currentZoom;

    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.5f;
    public float dampingSpeed = 1.0f;

    private GameObject player;
    private Transform initialTransform;
    private float initialZOffset;

    private Vector3 initialPosition;
    private float currentShakeDuration;
    private bool isShaking = false;

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
        initialPosition = transform.localPosition;

        // Set the initial zoom level to the maximum zoom
        currentZoom = maxZoom;
    }

    // Update is called once per frame
    void Update()
    {
        if (isShaking)
        {
            if (currentShakeDuration > 0)
            {
                // Apply camera shake using Random.insideUnitSphere for random shaking
                transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;

                // Decrease the remaining shake duration over time
                currentShakeDuration -= Time.deltaTime * dampingSpeed;
            }
            else
            {
                // Reset the camera position after the shake ends
                transform.localPosition = initialPosition;
                isShaking = false; // Stop shaking
            }
        }
        else
        {
            FollowPlayer();
        }
    }

    private void HandleZoom()
    {
        // Check if the Control key is held down
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Get the scroll wheel input (positive for up, negative for down)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            // Adjust the current zoom level based on the scroll input
            currentZoom -= scrollInput * zoomSensitivity;

            // Clamp the zoom level to ensure it's within the min and max limits
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            // Apply the zoom by adjusting the camera's position
            AdjustCameraZoom();
        }
    }

    private void AdjustCameraZoom()
    {
        // Assuming the camera is always looking at the player, adjust its distance
        // Here, we adjust the camera position along its forward axis (relative to its current rotation)
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -currentZoom);
    }

    void FollowPlayer()
    {
        transform.SetPositionAndRotation(new Vector3(player.transform.position.x,
            transform.position.y,
            player.transform.position.z + initialZOffset),
            initialTransform.rotation);

        Vector3 cameraCenter = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
        Vector3 playerPositionInCamera = player.transform.position - cameraCenter;

        if (Mathf.Abs(playerPositionInCamera.x) > deadZoneSize.x / 2 || Mathf.Abs(playerPositionInCamera.z) > deadZoneSize.y / 2)
        {
            Vector3 targetPosition = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z + initialZOffset);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
        }
    }

    // Public method to trigger the camera shake
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        currentShakeDuration = shakeDuration;
        isShaking = true;
        initialPosition = transform.localPosition; // Re-save the initial position in case camera moved
    }

    // Draw the dead zone in the scene view for debugging purposes
    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(transform.position.x, player.transform.position.y, transform.position.z),
                new Vector3(deadZoneSize.x, 0, deadZoneSize.y));
        }
    }
}
