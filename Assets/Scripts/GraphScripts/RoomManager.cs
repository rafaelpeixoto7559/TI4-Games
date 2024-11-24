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

    private GameObject[] roomInstances;

    [Header("Boss Room Prefab")]
    public GameObject bossRoomPrefab;

    [Header("Music Settings")]
    public AudioClip bossMusicClip;  // Música do boss


    private GameObject[] roomPrefabs;

    [Header("Player Settings")]
    public GameObject playerPrefab; // Prefab do jogador
    public Vector3 playerSpawnOffset = Vector3.zero; // Offset para spawnar o jogador dentro da sala

    [Header("Graph Settings")]
    public int numberOfRooms = 7;  // Número de salas regulares (sem incluir a sala do Boss)

    private int totalRooms;

    [Header("Camera Settings")]
    public Camera mainCamera; // Referência para a câmera principal

    private Graph graph;
    private List<RoomState> rooms = new List<RoomState>();  // Lista de estados das salas

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
        roomPrefabs = new GameObject[4]; // Agora temos 4 tipos de salas
        roomPrefabs[(int)RoomType.Sala1] = room1Prefab;
        roomPrefabs[(int)RoomType.Sala2] = room2Prefab;
        roomPrefabs[(int)RoomType.Sala3] = room3Prefab;
        roomPrefabs[(int)RoomType.BossRoom] = bossRoomPrefab;

        totalRooms = numberOfRooms + 1; // Incluindo a sala do Boss

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Nenhuma câmera principal encontrada. Por favor, atribua uma câmera no RoomManager.");
                return;
            }
        }

        // Corrigido aqui: usando o array de roomInstances
        graph = new Graph(totalRooms); // Passa as salas instanciadas para o grafo
        graph.GenerateConnectedGraph();
        graph.AssignRoomTypes();

        InstantiateRoomsWithTypes();

        roomInstances = GetRoomInstancesArray();

        graph.UpdateRoomInstances(roomInstances);
        // Alinha e conecta as portas
        graph.AlignAndConnectDoors(0); // Começa pela sala 0

        // Representação DOT do grafo (opcional)
        string dot = graph.ToDotFormat();
        Debug.Log("Representação do Grafo em DOT:\n" + dot);

        int bossRoomIndex = totalRooms - 1;
        int playerSpawnRoomIndex = graph.FindNodeFurthestFrom(bossRoomIndex);
        currentRoomIndex = playerSpawnRoomIndex;

        // Verifica se todas as portas estão alinhadas corretamente
        if (graph.VerifyAllDoorsAligned())
        {
            Debug.Log("Todas as portas estão alinhadas corretamente.");
        }
        else
        {
            Debug.LogError("Há portas desalinhadas nas salas. Verifique a lógica de rotacionamento.");
        }

        // Ativa apenas a sala atual
        ShowRoom(currentRoomIndex);

        // Spawna o jogador na sala inicial
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

    private GameObject[] GetRoomInstancesArray()
    {
        GameObject[] roomInstances = new GameObject[rooms.Count];
        for (int i = 0; i < rooms.Count; i++)
        {
            roomInstances[i] = rooms[i].roomInstance; // Possível NullReferenceException
        }
        return roomInstances;
    }



    void InstantiateRooms()
    {
        for (int i = 0; i < totalRooms; i++)
        {
            rooms.Add(null);
        }
    }

    void InstantiateRoomsWithTypes()
    {
        rooms.Clear(); // Limpa a lista antes de preenchê-la

        for (int i = 0; i < totalRooms; i++)
        {
            RoomType roomType = graph.roomTypes[i];
            GameObject roomPrefab = roomPrefabs[(int)roomType];
            GameObject roomInstance = Instantiate(roomPrefab);
            roomInstance.name = $"Room_{i}";

            // Desativa a sala imediatamente após a instanciação
            roomInstance.SetActive(false);

            // Cria o estado da sala e adiciona à lista
            RoomState roomState = new RoomState(roomInstance);
            rooms.Add(roomState);
        }
    }






    // Dentro da classe RoomManager
    public void GoToRoom(int roomIndex, DoorTrigger entranceDoor)
    {
        if (roomIndex == currentRoomIndex)
            return;

        Debug.Log($"Transitando da Sala {currentRoomIndex} para a Sala {roomIndex} via Porta {entranceDoor.doorDirection}");

        // Desativa a sala atual
        rooms[currentRoomIndex].roomInstance.GetComponent<RoomController>().DeactivateRoom();

        // Ativa a nova sala
        rooms[roomIndex].roomInstance.GetComponent<RoomController>().ActivateRoom();

        // Atualiza o índice da sala atual
        currentRoomIndex = roomIndex;

        // Reposiciona o jogador na nova sala usando o ponto de spawn associado à porta de saída
        RepositionPlayer(entranceDoor);

        // A sala é responsável por spawnar os inimigos na ativação
    }



    Vector3 GetRandomPositionInArea(Transform area)
    {
        // Supondo que o area tenha um Collider2D que define a área de spawn
        Collider2D collider = area.GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError("SpawnArea não possui um Collider2D.");
            return area.position;
        }

        Bounds bounds = collider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector3(x, y, area.position.z);
    }






    // Método para ativar apenas uma sala, desativando todas as outras
    void ShowRoom(int roomIndex)
    {
        // Desativa todas as salas
        foreach (var roomState in rooms)
        {
            roomState.roomInstance.SetActive(false);
        }

        // Ativa a sala especificada
        rooms[roomIndex].roomInstance.SetActive(true);
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
                GameObject room = rooms[currentRoomIndex].roomInstance;
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
                GameObject room = rooms[currentRoomIndex].roomInstance;
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
