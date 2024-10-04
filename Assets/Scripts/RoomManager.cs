// RoomManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RoomManager : MonoBehaviour
{
    // Singleton Instance
    public static RoomManager Instance { get; private set; }

    [Header("Room Prefabs")]
    public GameObject room1Prefab;  // Sala com 1 porta
    public GameObject room2Prefab;  // Sala com 2 portas
    public GameObject room3Prefab;  // Sala com 3 portas

    private GameObject[] roomPrefabs;

    [Header("Player Settings")]
    public GameObject playerPrefab; // Prefab do jogador
    public Vector3 playerSpawnOffset = new Vector3(0, -2, 0); // Offset para spawnar o jogador dentro da sala

    [Header("Graph Settings")]
    public int numberOfRooms = 15;  // Número total de salas

    [Header("Camera Settings")]
    public Camera mainCamera; // Referência para a câmera principal

    private Graph graph;
    private List<GameObject> rooms = new List<GameObject>();  // Lista de salas instanciadas

    private int currentRoomIndex = 0;  // Índice da sala atual em que o jogador está

    void Awake()
    {
        // Implementação do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Inicializa o array de prefabs de salas
        roomPrefabs = new GameObject[3]; // Supondo 3 tipos de salas
        roomPrefabs[(int)RoomType.Sala1] = room1Prefab;
        roomPrefabs[(int)RoomType.Sala2] = room2Prefab;
        roomPrefabs[(int)RoomType.Sala3] = room3Prefab;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Nenhuma câmera principal encontrada. Por favor, atribua uma câmera no RoomManager.");
                return;
            }
        }

         // Gera o grafo
        InstantiateRooms(); // Instancia as salas primeiro

        graph = new Graph(numberOfRooms, rooms.ToArray()); // Passa as salas instanciadas para o grafo
        graph.GenerateConnectedGraph();
        graph.AssignRoomTypes();
        InstantiateRoomsWithTypes();

        graph.UpdateRoomInstances(rooms.ToArray());
        // Alinha e conecta as portas
        graph.AlignAndConnectDoors(0); // Começa pela sala 0

        // Representação DOT do grafo (opcional)
        string dot = graph.ToDotFormat();
        Debug.Log("Representação do Grafo em DOT:\n" + dot);



 

        // Verifica se todas as portas estão alinhadas corretamente
        if (graph.VerifyAllDoorsAligned())
        {
            Debug.Log("Todas as portas estão alinhadas corretamente.");
        }
        else
        {
            Debug.LogError("Há portas desalinhadas nas salas. Verifique a lógica de rotacionamento.");
        }

        // Ativa apenas a primeira sala inicialmente
        ShowRoom(0);  // Começa no índice de sala 0

        // Spawna o jogador na primeira sala
        SpawnPlayer(0);

        // Atribui a câmera para seguir o jogador
        if (mainCamera != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
                if (cameraFollow != null)
                {
                    cameraFollow.SetTarget(player.transform);
                }
                else
                {
                    Debug.LogError("O componente CameraFollow não está anexado à câmera principal.");
                }
            }
            else
            {
                Debug.LogError("Jogador não encontrado para ser atribuído à câmera.");
            }
        }
    }

    void InstantiateRoomsWithTypes()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            RoomType roomType = graph.roomTypes[i];
            GameObject roomPrefab = roomPrefabs[(int)roomType];
            GameObject roomInstance = Instantiate(roomPrefab);
            roomInstance.name = $"Room_{i}";
            rooms[i] = roomInstance;
        }
    }

    void InstantiateRooms()
    {
        // Instancia e configura todas as salas
        for (int i = 0; i < numberOfRooms; i++)
        {
            // O tipo de sala será atribuído após a geração do grafo
            // Aqui, inicialmente, podemos instanciar qualquer sala ou deixar para instanciar após atribuir os tipos

            // Adiciona um placeholder na lista de salas
            rooms.Add(null);
        }
    }


    void UpdateDoorDirections(GameObject roomGO, int rotationIndex)
    {
        DoorTrigger[] doorTriggers = roomGO.GetComponentsInChildren<DoorTrigger>(true);
        foreach (var doorTrigger in doorTriggers)
        {
            // Atualiza currentDirection com base na rotação
            doorTrigger.currentDirection = DirectionHelper.RotateDirection(doorTrigger.doorDirection, rotationIndex);
            // Log opcional do resultado
            Debug.Log($"Porta {doorTrigger.doorDirection} rotacionada em {rotationIndex * 90} graus torna-se {doorTrigger.currentDirection} na {roomGO.name}");
        }
    }

    DoorTrigger GetDoorTrigger(GameObject room, DoorDirection doorDirection)
    {
        DoorTrigger[] doorTriggers = room.GetComponentsInChildren<DoorTrigger>(true); // Inclui objetos inativos
        foreach (var doorTrigger in doorTriggers)
        {
            if (doorTrigger.currentDirection == doorDirection)
            {
                return doorTrigger;
            }
        }
        Debug.LogError($"[GetDoorTrigger] DoorTrigger com direção {doorDirection} não encontrado na sala {room.name}. Portas disponíveis: {string.Join(", ", GetDoorDirectionsInRoom(room))}");
        return null;
    }

    IEnumerable<DoorDirection> GetDoorDirectionsInRoom(GameObject room)
    {
        DoorTrigger[] doorTriggers = room.GetComponentsInChildren<DoorTrigger>(true);
        foreach (var doorTrigger in doorTriggers)
        {
            yield return doorTrigger.currentDirection;
        }
    }

    public void GoToRoom(int roomIndex)
    {
        if (roomIndex == currentRoomIndex)
            return;

        Debug.Log($"Transitioning from Room {currentRoomIndex} to Room {roomIndex}");

        // Desativa a sala atual
        rooms[currentRoomIndex].SetActive(false);

        // Ativa a nova sala
        rooms[roomIndex].SetActive(true);

        // Atualiza o índice da sala atual
        currentRoomIndex = roomIndex;

        // Reposiciona o jogador na nova sala
        RepositionPlayer();
    }

    // Método para ativar apenas uma sala, desativando todas as outras
    void ShowRoom(int roomIndex)
    {
        // Desativa todas as salas
        foreach (var room in rooms)
        {
            room.SetActive(false);
        }

        // Ativa a sala especificada
        rooms[roomIndex].SetActive(true);
        currentRoomIndex = roomIndex;
    }

    // Método para spawnar o jogador na sala inicial
    void SpawnPlayer(int roomIndex)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab não está atribuído no RoomManager.");
            return;
        }

        // Verifica se já existe um Player na cena
        GameObject existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer == null)
        {
            // Instancia o Player
            GameObject player = Instantiate(playerPrefab, GetPlayerSpawnPosition(roomIndex), Quaternion.identity);
            player.name = "Player";
            Debug.Log("Player instanciado.");
        }
        else
        {
            // Reposiciona o Player existente
            existingPlayer.transform.position = GetPlayerSpawnPosition(roomIndex);
            Debug.Log("Player existente reposicionado.");
        }
    }

    // Método para reposicionar o jogador na nova sala
    void RepositionPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = GetPlayerSpawnPosition(currentRoomIndex);
            Debug.Log("Player reposicionado.");
        }
        else
        {
            Debug.LogError("Jogador não encontrado na cena.");
        }
    }

    // Método para obter a posição de spawn do jogador dentro de uma sala
    Vector3 GetPlayerSpawnPosition(int roomIndex)
    {
        GameObject room = rooms[roomIndex];
        // Supondo que o player deve spawnar no centro da sala com um offset específico
        return room.transform.position + playerSpawnOffset;
    }
}
