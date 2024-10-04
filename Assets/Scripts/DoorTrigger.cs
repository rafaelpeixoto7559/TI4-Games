using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorDirection doorDirection;
    public DoorDirection currentDirection;
    public int connectedRoomIndex = -1; // Initialize to -1

    private RoomManager roomManager;

    void Awake()
    {
        // Assign the RoomManager instance
        roomManager = RoomManager.Instance;
        if (roomManager == null)
        {
            Debug.LogError("RoomManager instance not found in DoorTrigger.");
        }

        // Initially, set currentDirection to doorDirection
        currentDirection = doorDirection;
    }

     private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (connectedRoomIndex != -1)
            {
                Debug.Log($"Player entered the door to Room {connectedRoomIndex}");
                RoomManager.Instance.GoToRoom(connectedRoomIndex);
            }
            else
            {
                Debug.LogError("Door has no connected room assigned.");
            }
        }
    }

    // Method to draw Gizmos to visualize the door in the Editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 2, 1)); // Simple representation of the door
    }
}
