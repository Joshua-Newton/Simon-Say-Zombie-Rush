using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)][SerializeField] float initialDistanceToPlayer = 10f;
    [Range(0, 90)][SerializeField] float angleToPlayer = 75f;

    [Header("Zoom Settings")]
    [Range(0, 10f)][SerializeField] private float minZoom = 5f;    // Minimum zoom distance
    [Range(0, 10f)][SerializeField] private float maxZoom = 5f;   // Maximum zoom distance
    [Range(0, 1f)][SerializeField] private float zoomSensitivity = 0.05f; // How sensitive the zoom control is

    [Header("Camera Shake Settings")]
    [Range(0, 1f)] [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.5f;
    [Range(0, 1f)] [SerializeField] float shakeFrameDuration = 0.01f;

    private GameObject player;
    private Vector3 shakeOffset = Vector3.zero;
    private float initialZOffset;
    private float currentZOffset;
    private float currentDistanceToPlayer;
    private bool isShaking;
    private bool isRandomizingShake;
    void Start()
    {
        player = GameManager.instance.player;

        Quaternion newRotation = Quaternion.identity;
        newRotation = Quaternion.Euler(newRotation.x + angleToPlayer, newRotation.y, newRotation.z);
        Vector3 newPosition = player.transform.position - (transform.TransformDirection(Vector3.forward) * initialDistanceToPlayer);
        transform.SetPositionAndRotation(newPosition, newRotation);

        initialZOffset = transform.position.z - player.transform.position.z;
        currentZOffset = initialZOffset;
        currentDistanceToPlayer = initialDistanceToPlayer;
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
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetButton("EnableZoom") && Mathf.Abs(scrollInput) > 0f)
        {
            if (scrollInput > 0f)
            {
                currentDistanceToPlayer -= zoomSensitivity;
                UpdateCameraDistance();
            }
            else if(scrollInput < 0f)
            {
                currentDistanceToPlayer += zoomSensitivity;
                UpdateCameraDistance();
            }

        }
    }

    private void UpdateCameraDistance()
    {
        currentDistanceToPlayer = Mathf.Clamp(currentDistanceToPlayer, initialDistanceToPlayer - minZoom, initialDistanceToPlayer + maxZoom);
        transform.position = player.transform.position - (transform.TransformDirection(Vector3.forward) * currentDistanceToPlayer);
        currentZOffset = transform.position.z - player.transform.position.z;
    }

    void FollowPlayer()
    {
        Vector3 cameraPosition = player.transform.position - (transform.TransformDirection(Vector3.forward) * currentDistanceToPlayer);
        if (isShaking)
        {
            cameraPosition += shakeOffset;
        }

        // Make the camera follow the player
        transform.SetPositionAndRotation(cameraPosition, transform.rotation);
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
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
        shakeOffset = Vector3.zero;
        isRandomizingShake = false;
    }

    public void ZeroShakeOffset()
    {
        shakeOffset = Vector3.zero;
    }
}
