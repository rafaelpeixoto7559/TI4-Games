// RoomManager.cs
using System.Collections.Generic;
using UnityEngine;

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
    public Vector3 playerSpawnOffset = Vector3.zero; // Offset para spawnar o jogador dentro da sala

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
        SpawnPlayer(null); // Usa null para spawnar no centro ou posição padrão

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
            // Adiciona um placeholder na lista de salas
            rooms.Add(null);
        }
    }

    /// <summary>
    /// Transita para a sala especificada, posicionando o jogador no ponto de spawn associado à porta de saída.
    /// </summary>
    /// <param name="roomIndex">Índice da sala para a qual transitar.</param>
    /// <param name="entranceDoor">Referência ao DoorTrigger pela qual o jogador está entrando.</param>
    public void GoToRoom(int roomIndex, DoorTrigger entranceDoor)
    {
        if (roomIndex == currentRoomIndex)
            return;

        Debug.Log($"Transitando da Sala {currentRoomIndex} para a Sala {roomIndex} via Porta {entranceDoor.doorDirection}");

        // Desativa a sala atual
        rooms[currentRoomIndex].SetActive(false);

        // Ativa a nova sala
        rooms[roomIndex].SetActive(true);

        // Atualiza o índice da sala atual
        currentRoomIndex = roomIndex;

        // Reposiciona o jogador na nova sala usando o ponto de spawn associado à porta de saída
        RepositionPlayer(entranceDoor);
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

    /// <summary>
    /// Spawna o jogador na sala inicial ou em transições.
    /// </summary>
    /// <param name="entranceDoor">Referência ao DoorTrigger pela qual o jogador está entrando. Null para spawn inicial.</param>
    void SpawnPlayer(DoorTrigger entranceDoor)
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
            Vector3 spawnPosition = Vector3.zero;
            if (entranceDoor != null && entranceDoor.connectedDoorTrigger != null && entranceDoor.connectedDoorTrigger.spawnPoint != null)
            {
                // Usa o spawnPoint da porta de saída na nova sala
                spawnPosition = entranceDoor.connectedDoorTrigger.spawnPoint.position + playerSpawnOffset;
            }
            else
            {
                // Spawn padrão no centro da sala
                GameObject room = rooms[currentRoomIndex];
                if (room != null)
                {
                    spawnPosition = room.transform.position + playerSpawnOffset;
                }
                else
                {
                    Debug.LogError("Sala atual não está instanciada corretamente.");
                }
            }

            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            player.name = "Player";
            Debug.Log("Player instanciado.");
        }
        else
        {
            // Reposiciona o Player existente
            RepositionPlayer(entranceDoor);
            Debug.Log("Player existente reposicionado.");
        }
    }

    /// <summary>
    /// Reposiciona o jogador na nova sala usando o ponto de spawn associado à porta de saída.
    /// </summary>
    /// <param name="entranceDoor">Referência ao DoorTrigger pela qual o jogador está entrando.</param>
    void RepositionPlayer(DoorTrigger entranceDoor)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 spawnPosition = Vector3.zero;
            if (entranceDoor != null && entranceDoor.connectedDoorTrigger != null && entranceDoor.connectedDoorTrigger.spawnPoint != null)
            {
                // Usa o spawnPoint da porta de saída na nova sala
                spawnPosition = entranceDoor.connectedDoorTrigger.spawnPoint.position;
            }
            else
            {
                // Spawn padrão no centro da sala
                GameObject room = rooms[currentRoomIndex];
                if (room != null)
                {
                    spawnPosition = room.transform.position;
                }
                else
                {
                    Debug.LogError("Sala atual não está instanciada corretamente.");
                }
            }

            player.transform.position = spawnPosition;
            Debug.Log("Player reposicionado.");
        }
        else
        {
            Debug.LogError("Jogador não encontrado na cena.");
        }
    }
}