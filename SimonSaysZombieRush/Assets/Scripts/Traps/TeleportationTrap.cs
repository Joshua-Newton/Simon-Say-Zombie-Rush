using UnityEngine;

public class TeleportationTrap : MonoBehaviour
{
    [SerializeField] private Transform[] teleportDestinations = new Transform[3];  // Serialized field for destination points

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Generate a random destination index
            int destinationIndex = Random.Range(0, teleportDestinations.Length);

            // Get the selected destination transform
            Transform selectedDestination = teleportDestinations[destinationIndex];

            // Teleport the player
            other.transform.position = selectedDestination.position;
        }
    }
}
