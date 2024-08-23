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
    [Range(1, 100)][SerializeField] private float minZoom;    // Minimum zoom distance
    [Range(1, 100)][SerializeField] private float maxZoom;   // Maximum zoom distance
    [Range(1, 100)][SerializeField] private float zoomSensitivity; // How sensitive the zoom control is
    private float currentZoom;

    [Header("Camera Shake Settings")]
    [Range(0, 1f)] [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.5f;
    [SerializeField] private float dampingSpeed = 1.0f;
    [Range(0, 1f)] [SerializeField] float shakeFrameDuration = 0.01f;

    private GameObject player;
    private Vector3 initialPosition;
    private Vector3 shakeOffset = Vector3.zero;
    private float initialZOffset;
    private float currentShakeDuration;
    private bool isShaking;
    private bool isRandomizingShake;
    void Start()
    {
        player = GameManager.instance.player;

        Quaternion newRotation = Quaternion.identity;
        newRotation = Quaternion.Euler(newRotation.x + angleToPlayer, newRotation.y, newRotation.z);
        transform.SetPositionAndRotation(player.transform.position, newRotation);
        transform.position -= transform.TransformDirection(Vector3.forward) * distanceToPlayer;

        initialZOffset = transform.position.z - player.transform.position.z;
        initialPosition = transform.position;

        // Set the initial zoom level based on the initial distance to the player
        currentZoom = distanceToPlayer;
    }

    void Update()
    {
        if(GameManager.instance != null && !GameManager.instance.isPaused)
        {
            HandleZoom(); // Handle zoom input
            FollowPlayer();
            if (isShaking && !isRandomizingShake)
            {
                StartCoroutine(RandomizeShakeOffset());
            }
        }
    }

    private void HandleZoom()
    {
        // Check if the Control key is held down and if there is scroll input
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0f)
        {
            // Get the scroll wheel input (positive for up, negative for down)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            // Adjust the target zoom level based on the scroll input
            currentZoom -= scrollInput * zoomSensitivity;

            // Clamp the zoom level to ensure it's within the min and max limits
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }

        // Apply the zoom smoothly by adjusting the camera's position
        AdjustCameraZoom();
    }

    private void AdjustCameraZoom()
    {
        // Smoothly interpolate the zoom level (distance to the player)
        float smoothZoom = Mathf.Lerp(Vector3.Distance(transform.position, player.transform.position), currentZoom, Time.deltaTime * 5f);

        // Calculate the zoom direction from the camera to the player
        Vector3 zoomDirection = (transform.position - player.transform.position).normalized;

        // Calculate the target position based on the smoothed zoom level
        Vector3 targetZoomPosition = player.transform.position + zoomDirection * smoothZoom;

        // Smoothly interpolate the camera's position towards the target zoom position
        transform.position = Vector3.Lerp(transform.position, targetZoomPosition, Time.deltaTime * 5f);
    }

    void FollowPlayer()
    {
        Vector3 cameraPosition = new Vector3(player.transform.position.x,
            transform.position.y,
            player.transform.position.z + initialZOffset);
        
        cameraPosition += shakeOffset;

        // Make the camera follow the player
        transform.SetPositionAndRotation(cameraPosition, transform.rotation);
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        currentShakeDuration = shakeDuration;
        StartCoroutine(EnableCameraShake());
    }

    IEnumerator EnableCameraShake()
    {
        isShaking = true;
        yield return new WaitForSeconds(shakeDuration);
        StopCoroutine(RandomizeShakeOffset());
        shakeOffset = Vector3.zero;
        isShaking = false;
    }

    IEnumerator RandomizeShakeOffset()
    {
        isRandomizingShake = true;
        shakeOffset = Random.insideUnitSphere * shakeMagnitude;
        yield return new WaitForSeconds(shakeFrameDuration);
        isRandomizingShake = false;
    }

    public void ZeroShakeOffset()
    {
        shakeOffset = Vector3.zero;
    }
}
