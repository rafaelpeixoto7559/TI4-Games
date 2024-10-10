// DoorTrigger.cs
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorDirection doorDirection; // Direção original da porta (e.g., East, West)
    public DoorDirection currentDirection; // Direção atual após rotação
    public int connectedRoomIndex = -1; // Índice da sala conectada
    public DoorTrigger connectedDoorTrigger; // Referência ao DoorTrigger conectado

    public Transform spawnPoint; // Referência ao ponto de spawn associado à porta

    private RoomManager roomManager;

    void Awake()
    {
        // Atribui a instância do RoomManager
        roomManager = RoomManager.Instance;
        if (roomManager == null)
        {
            Debug.LogError("Instância do RoomManager não encontrada em DoorTrigger.");
        }

        // Inicialmente, define currentDirection como doorDirection
        currentDirection = doorDirection;

        // Obtém o spawnPoint (filho) se não estiver atribuído
        if (spawnPoint == null)
        {
            Transform childSpawn = transform.Find("SpawnPoint");
            if (childSpawn != null)
            {
                spawnPoint = childSpawn;
            }
            else
            {
                Debug.LogError($"SpawnPoint não encontrado como filho de {gameObject.name}");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (connectedRoomIndex != -1 && connectedDoorTrigger != null)
            {
                Debug.Log($"Player entrou pela porta {currentDirection} para a Sala {connectedRoomIndex}");
                roomManager.GoToRoom(connectedRoomIndex, this); // Passa a própria porta como referência
            }
            else
            {
                Debug.LogError("A porta não tem uma sala conectada ou o DoorTrigger conectado não está atribuído.");
            }
        }
    }

    // Método para desenhar Gizmos no Editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 2, 1)); // Representação simples da porta

        if (spawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(spawnPoint.position, 0.2f); // Representação do ponto de spawn
        }
    }
}
