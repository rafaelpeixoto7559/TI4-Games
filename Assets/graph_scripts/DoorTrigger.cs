// DoorTrigger.cs
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public RoomManager roomManager; // Referência ao RoomManager
    public int connectedRoomIndex;  // Índice da sala para a qual essa porta leva

    // Direção atual da porta após rotação
    public DoorDirection currentDirection;

    // Quando o jogador entrar no colisor da porta
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Verifica se é o jogador
        {
            Debug.Log($"Player entrou na porta para a Sala {connectedRoomIndex}");
            if (roomManager != null)
            {
                roomManager.GoToRoom(connectedRoomIndex); // Chama o RoomManager para trocar de sala
            }
            else
            {
                Debug.LogError("RoomManager não está atribuído na porta.");
            }
        }
    }

    // Método para desenhar Gizmos para visualizar a porta no Editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 2, 1)); // Representação simples da porta
    }
}
