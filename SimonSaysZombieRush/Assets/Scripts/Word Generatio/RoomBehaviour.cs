using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the visibility of walls and doors in a room, supporting rooms with any number of walls and doors.
/// </summary>
public class RoomBehaviour : MonoBehaviour
{
    // Wall-Door pairs with corresponding positions in the room
    [System.Serializable]
    public class WallDoorPair
    {
        public GameObject wall;
        public GameObject door;
    }

    [SerializeField]
    private List<WallDoorPair> wallDoorPairs = new List<WallDoorPair>(); // List of wall-door pairs

    /// <summary>
    /// Updates the room's doors and walls based on the given status array.
    /// </summary>
    /// <param name="status">Array indicating whether a door should be open (true) or closed (false) for each wall-door pair.</param>
    public void UpdateRoom(bool[] status)
    {
        if (status == null || status.Length != wallDoorPairs.Count)
        {
            return;
        }

        for (int i = 0; i < wallDoorPairs.Count; i++)
        {
            WallDoorPair pair = wallDoorPairs[i];

            if (pair.door == null || pair.wall == null)
            {
                continue;
            }

            if (pair.door.activeSelf != status[i])
            {
                pair.door.SetActive(status[i]);      // Activate the door if the status is true
            }

            if (pair.wall.activeSelf != !status[i])
            {
                pair.wall.SetActive(!status[i]);     // Activate the wall if the status is false
            }
        }
    }
}
