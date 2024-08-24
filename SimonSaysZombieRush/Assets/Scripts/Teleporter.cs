using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private bool instantTeleport;
    [SerializeField] private bool randomTeleport;
    [SerializeField] private bool buttonTeleport;
    [SerializeField] private string buttonName;
    [SerializeField] private bool delayedTeleport;
    [SerializeField] private float teleportTime = 3f;
    [SerializeField] private Transform[] destinationPad;
    [SerializeField] private float teleportationHeightOffset = 1f;
    [SerializeField] private bool allowEntry = true;
    [SerializeField] private bool allowExit = true;

    private float curTeleportTime;
    private bool inside;
    private Transform subject;
    private bool arrived;

    public AudioSource teleportSound;
    public AudioSource teleportPadSound;
    public bool teleportPadOn = true;

    // Array of tags that are allowed to teleport (e.g., "Player" and "Zombies")
    [SerializeField] private string[] allowedTags = { "Player", "Zombies" };

    void Start()
    {
        // Initialize the countdown timer
        curTeleportTime = teleportTime;
    }

    void Update()
    {
        // Check if an object is inside and teleport is ready
        if (inside && teleportPadOn && !arrived)
        {
            Teleport();
        }
    }

    void Teleport()
    {
        if (subject == null) return; // Ensure subject is not null before teleporting

        if (instantTeleport)
        {
            ExecuteTeleport(randomTeleport);
        }
        else if (delayedTeleport)
        {
            curTeleportTime -= Time.deltaTime; // Use Time.deltaTime for smooth countdown

            if (curTeleportTime <= 0f)
            {
                curTeleportTime = teleportTime; // Reset the countdown
                ExecuteTeleport(randomTeleport);
            }
        }
        else if (buttonTeleport && Input.GetButtonDown(buttonName))
        {
            if (delayedTeleport)
            {
                curTeleportTime -= Time.deltaTime;

                if (curTeleportTime <= 0f)
                {
                    curTeleportTime = teleportTime;
                    ExecuteTeleport(randomTeleport);
                }
            }
            else
            {
                ExecuteTeleport(randomTeleport);
            }
        }
    }

    void ExecuteTeleport(bool random)
    {
        if (destinationPad == null || destinationPad.Length == 0) return; // Safety check for valid destinations

        int chosenPad = random ? Random.Range(0, destinationPad.Length) : 0;

        if (destinationPad[chosenPad] == null) return; // Ensure chosen destination is valid

        // Temporarily disable the subject's collider to prevent getting stuck in the pad's collider
        Collider subjectCollider = subject.GetComponent<Collider>();
        if (subjectCollider != null)
        {
            subjectCollider.enabled = false;
        }

        // Teleport the subject to the destination pad, applying height offset
        subject.position = destinationPad[chosenPad].position + new Vector3(0, teleportationHeightOffset, 0);

        // Play teleport sound if available
        if (teleportSound != null)
        {
            teleportSound.Play();
        }

        // Re-enable the subject's collider after teleporting
        if (subjectCollider != null)
        {
            StartCoroutine(EnableColliderAfterDelay(subjectCollider, 0.1f));
        }

        // Mark the destination pad to prevent back teleportation
        destinationPad[chosenPad].GetComponent<Teleporter>().arrived = true;
    }

    void OnTriggerEnter(Collider trig)
    {
        if (!allowEntry) return;

        if (IsTeleportable(trig))
        {
            subject = trig.transform; // Set the subject to the object entering
            inside = true;
            arrived = !buttonTeleport; // Ready for teleport if not using button-based teleport
        }
    }

    void OnTriggerExit(Collider trig)
    {
        if (!allowExit) return;

        if (IsTeleportable(trig))
        {
            inside = false;
            curTeleportTime = teleportTime; // Reset countdown time

            if (trig.transform == subject)
            {
                arrived = false; // Reset arrived status
                subject = null; // Clear the subject
            }
        }
    }

    bool IsTeleportable(Collider trig)
    {
        // Check if the object's tag matches any of the allowed tags
        foreach (string tag in allowedTags)
        {
            if (trig.CompareTag(tag))
            {
                return true; // If the object has a matching tag, allow teleportation
            }
        }
        return false; // If no matching tag is found, deny teleportation
    }

    IEnumerator EnableColliderAfterDelay(Collider col, float delay)
    {
        yield return new WaitForSeconds(delay);
        col.enabled = true; // Re-enable the collider after a short delay
    }
}
