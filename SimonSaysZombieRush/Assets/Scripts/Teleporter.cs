using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Teleporter Settings")]
    [SerializeField] private Teleporter targetTeleporter; // The target teleporter to teleport to
    [SerializeField] private bool isOneWay = false; // One-way teleport option
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Key to trigger teleportation

    private bool playerInRange = false;
    private GameObject player;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        if (targetTeleporter != null && (!isOneWay || playerInRange))
        {
            player.transform.position = targetTeleporter.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }
}
