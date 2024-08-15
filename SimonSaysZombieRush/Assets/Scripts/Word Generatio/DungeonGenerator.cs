using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a dungeon by creating a maze and placing rooms based on specified rules.
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    /// <summary>
    /// Represents a cell in the dungeon grid.
    /// </summary>
    public class Cell
    {
        public bool visited = false;       // Tracks if the cell has been visited during maze generation
        public bool[] status = new bool[4]; // Represents open paths: 0 - Up, 1 - Down, 2 - Right, 3 - Left
    }

    [System.Serializable]
    public class Rule
    {
        public GameObject room;               // The room prefab to be placed
        public Vector2Int minPosition;        // Minimum position for this room to spawn
        public Vector2Int maxPosition;        // Maximum position for this room to spawn
        public bool obligatory;               // If true, the room must spawn within the defined range

        /// <summary>
        /// Determines if a room can or must spawn at the given position.
        /// </summary>
        public int ProbabilityOfSpawning(int x, int y)
        {
            if (x >= minPosition.x && x <= maxPosition.x && y >= minPosition.y && y <= maxPosition.y)
            {
                return obligatory ? 2 : 1;
            }

            return 0;
        }
    }

    public Vector2Int size;    // The dimensions of the dungeon grid
    public int startPos = 0;   // Starting position in the grid for maze generation
    public Rule[] rooms;       // Array of rules defining room placement
    public Vector2 offset;     // Offset for positioning rooms in the world space

    private List<Cell> board;  // List representing the dungeon grid

    // Start is called before the first frame update
    private void Start()
    {
        if (ValidateInputs())
        {
            StartCoroutine(GenerateDungeonCoroutine());
        }
    }

    /// <summary>
    /// Validates the input parameters for dungeon generation.
    /// </summary>
    private bool ValidateInputs()
    {
        if (size.x <= 0 || size.y <= 0)
        {
            Debug.LogError("Dungeon size must be greater than zero.");
            return false;
        }

        if (rooms == null || rooms.Length == 0)
        {
            Debug.LogError("Rooms array cannot be null or empty.");
            return false;
        }

        if (startPos < 0 || startPos >= size.x * size.y)
        {
            Debug.LogError("Invalid start position.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Generates the dungeon asynchronously to avoid freezing the game.
    /// </summary>
    private IEnumerator GenerateDungeonCoroutine()
    {
        InitializeBoard();
        yield return GenerateMazeCoroutine();
        PlaceRooms();
    }

    /// <summary>
    /// Initializes the dungeon grid.
    /// </summary>
    private void InitializeBoard()
    {
        board = new List<Cell>();

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                board.Add(new Cell());
            }
        }
    }

    /// <summary>
    /// Generates the maze using a depth-first search algorithm.
    /// </summary>
    private IEnumerator GenerateMazeCoroutine()
    {
        Stack<int> path = new Stack<int>();
        int currentCell = startPos;

        while (true)
        {
            board[currentCell].visited = true;

            if (IsMazeComplete(currentCell))
                break;

            List<int> neighbors = GetUnvisitedNeighbors(currentCell);

            if (neighbors.Count == 0)
            {
                if (path.Count == 0) break;
                currentCell = path.Pop();
            }
            else
            {
                path.Push(currentCell);
                currentCell = MoveToNeighboringCell(currentCell, neighbors);
            }

            yield return null; // Allow the game to update between iterations
        }
    }

    /// <summary>
    /// Checks if the maze generation is complete.
    /// </summary>
    private bool IsMazeComplete(int currentCell)
    {
        return currentCell == board.Count - 1;
    }

    /// <summary>
    /// Retrieves a list of unvisited neighboring cells.
    /// </summary>
    private List<int> GetUnvisitedNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        int up = cell - size.x;
        int down = cell + size.x;
        int right = cell + 1;
        int left = cell - 1;

        // Check if 'up' is within bounds and unvisited
        if (up >= 0 && up < board.Count && !board[up].visited)
        {
            neighbors.Add(up);
        }

        // Check if 'down' is within bounds and unvisited
        if (down >= 0 && down < board.Count && !board[down].visited)
        {
            neighbors.Add(down);
        }

        // Check if 'right' is within bounds and unvisited
        if (right % size.x != 0 && right >= 0 && right < board.Count && !board[right].visited)
        {
            neighbors.Add(right);
        }

        // Check if 'left' is within bounds and unvisited
        if (left % size.x != size.x - 1 && left >= 0 && left < board.Count && !board[left].visited)
        {
            neighbors.Add(left);
        }

        return neighbors;
    }


    /// <summary>
    /// Moves to a neighboring cell and updates the cell status.
    /// </summary>
    private int MoveToNeighboringCell(int currentCell, List<int> neighbors)
    {
        int newCell = neighbors[Random.Range(0, neighbors.Count)];
        UpdateCellStatus(currentCell, newCell);
        return newCell;
    }

    /// <summary>
    /// Updates the status of the current and new cell.
    /// </summary>
    private void UpdateCellStatus(int currentCell, int newCell)
    {
        if (newCell > currentCell)
        {
            if (newCell - 1 == currentCell)
            {
                board[currentCell].status[2] = true; // Right
                board[newCell].status[3] = true; // Left
            }
            else
            {
                board[currentCell].status[1] = true; // Down
                board[newCell].status[0] = true; // Up
            }
        }
        else
        {
            if (newCell + 1 == currentCell)
            {
                board[currentCell].status[3] = true; // Left
                board[newCell].status[2] = true; // Right
            }
            else
            {
                board[currentCell].status[0] = true; // Up
                board[newCell].status[1] = true; // Down
            }
        }
    }

    /// <summary>
    /// Places rooms on the dungeon grid based on the maze layout.
    /// </summary>
    private void PlaceRooms()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                int index = i + j * size.x;
                Cell currentCell = board[index];

                if (currentCell.visited)
                {
                    int roomIndex = SelectRoom(i, j);
                    InstantiateRoom(roomIndex, i, j, currentCell);
                }
            }
        }
    }

    /// <summary>
    /// Selects a room based on the current position and rules.
    /// </summary>
    private int SelectRoom(int i, int j)
    {
        List<int> availableRooms = new List<int>();
        int selectedRoom = -1;

        for (int k = 0; k < rooms.Length; k++)
        {
            int probability = rooms[k].ProbabilityOfSpawning(i, j);

            if (probability == 2)
            {
                return k;
            }
            else if (probability == 1)
            {
                availableRooms.Add(k);
            }
        }

        if (availableRooms.Count > 0)
        {
            selectedRoom = availableRooms[Random.Range(0, availableRooms.Count)];
        }
        else
        {
            selectedRoom = 0; // Default to the first room if none are available
        }

        return selectedRoom;
    }

    /// <summary>
    /// Instantiates the selected room in the scene.
    /// </summary>
    private void InstantiateRoom(int roomIndex, int i, int j, Cell currentCell)
    {
        Vector3 position = new Vector3(i * offset.x, 0, -j * offset.y);
        var newRoom = Instantiate(rooms[roomIndex].room, position, Quaternion.identity, transform).GetComponent<RoomBehaviour>();
        newRoom.UpdateRoom(currentCell.status);
        newRoom.name += $" {i}-{j}";  // Name the room based on its grid position
    }
}
