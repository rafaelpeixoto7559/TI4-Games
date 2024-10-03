using System.Collections;
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

    [Header("Player Settings")]
    public GameObject playerPrefab; // Prefab do jogador
    public Vector3 playerSpawnOffset = new Vector3(0, -2, 0); // Offset para spawn do jogador dentro da sala

    [Header("Spawn Settings")]
    public Vector3 roomSpawnBasePosition = Vector3.zero; // Posição base para spawn das salas
    public float roomSpacing = 20f; // Espaçamento entre as salas

    [Header("Graph Settings")]
    public int numberOfRooms = 10;  // Número total de salas
    public int maxDegree = 4; // Grau máximo por vértice

    [Header("Camera Settings")]
    public Camera mainCamera; // Referência à câmera principal

    private Graph graph;  // Grafo gerado
    private List<GameObject> rooms = new List<GameObject>();  // Lista de salas instanciadas

    private int currentRoomIndex = 0;  // Índice da sala em que o player está atualmente

    // Mapeamento de portas disponíveis por sala para evitar sobreposições
    private Dictionary<int, int> roomDoorCounters = new Dictionary<int, int>();

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
        // Verificar se a referência da câmera foi atribuída
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Nenhuma câmera principal encontrada. Por favor, atribua uma câmera no RoomManager.");
                return;
            }
        }

        // Gerar o grafo
        graph = new Graph(numberOfRooms);
        graph.GenerateConnectedGraph();

        // Instanciar as salas com base no grafo gerado
        GenerateRooms();

        // Conectar as salas com portas
        ConnectRooms();

        // Ativar apenas a primeira sala inicialmente
        ShowRoom(0);  // Começa na sala de índice 0

        // Spawn do jogador na primeira sala
        SpawnPlayer(0);

        // Atribuir a câmera para seguir o jogador
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
                Debug.LogError("Jogador não encontrado para atribuir à câmera.");
            }
        }
    }

    // Método para gerar salas de acordo com o grau dos vértices no grafo
    void GenerateRooms()
    {
        for(int i = 0; i < numberOfRooms; i++)
        {
            int degree = graph.adjacencyList[i].Count;

            GameObject roomPrefabToUse;
            if(degree == 1)
                roomPrefabToUse = room1Prefab;
            else if(degree == 2)
                roomPrefabToUse = room2Prefab;
            else
                roomPrefabToUse = room3Prefab;

            // Calcula a posição padronizada para a sala
            // Aqui, posicionamos as salas em um grid 5x2. Ajuste conforme necessário.
            Vector3 roomPosition = roomSpawnBasePosition + new Vector3((i % 5) * roomSpacing, (i / 5) * roomSpacing, 0);

            GameObject room = Instantiate(roomPrefabToUse, roomPosition, Quaternion.identity);
            room.name = "Room " + i;
            room.SetActive(false);  // Desativa a sala inicialmente
            rooms.Add(room);

            // Assign roomManager to all doors in the room
            DoorTrigger[] doors = room.GetComponentsInChildren<DoorTrigger>();
            foreach(var door in doors)
            {
                door.roomManager = this;
            }

            // Inicializa o contador de portas para a sala
            roomDoorCounters[i] = 0;
        }
    }

    // Método para conectar as salas com portas com base nas arestas do grafo
    void ConnectRooms()
    {
        foreach(var edge in graph.Edges)
        {
            GameObject roomA = rooms[edge.Source];
            GameObject roomB = rooms[edge.Destination];

            DoorTrigger[] doorsA = roomA.GetComponentsInChildren<DoorTrigger>();
            DoorTrigger[] doorsB = roomB.GetComponentsInChildren<DoorTrigger>();

            // Verifica se as salas têm portas disponíveis para conexão
            if(doorsA.Length > roomDoorCounters[edge.Source] && doorsB.Length > roomDoorCounters[edge.Destination])
            {
                DoorTrigger doorA = doorsA[roomDoorCounters[edge.Source]];
                DoorTrigger doorB = doorsB[roomDoorCounters[edge.Destination]];

                // Conecta doorA a doorB
                doorA.connectedRoomIndex = edge.Destination;
                doorB.connectedRoomIndex = edge.Source;

                Debug.Log($"Conectando Porta {roomDoorCounters[edge.Source] + 1} de Sala {edge.Source} à Porta correspondente de Sala {edge.Destination}");

                // Incrementa o contador de portas usadas
                roomDoorCounters[edge.Source]++;
                roomDoorCounters[edge.Destination]++;
            }
            else
            {
                Debug.LogWarning($"Salas {edge.Source} ou {edge.Destination} não possuem portas suficientes para conectar.");
            }
        }
    }

    // Método para trocar de sala
    public void GoToRoom(int roomIndex)
    {
        if(roomIndex == currentRoomIndex)
            return;

        Debug.Log($"Trocando da Sala {currentRoomIndex} para a Sala {roomIndex}");

        // Desativar a sala atual
        rooms[currentRoomIndex].SetActive(false);

        // Ativar a nova sala
        rooms[roomIndex].SetActive(true);

        // Atualizar o índice da sala atual
        currentRoomIndex = roomIndex;

        // Reposicionar o jogador na nova sala
        RepositionPlayer();
    }

    // Método para ativar apenas uma sala, desativando todas as outras
    void ShowRoom(int roomIndex)
    {
        foreach(var room in rooms)
        {
            room.SetActive(false);
        }

        rooms[roomIndex].SetActive(true);
        currentRoomIndex = roomIndex;
    }

    // Método para spawnar o jogador na sala inicial
    void SpawnPlayer(int roomIndex)
    {
        if(playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab não está atribuído no RoomManager.");
            return;
        }

        // Verifica se já existe um Player na cena
        GameObject existingPlayer = GameObject.FindWithTag("Player");
        if(existingPlayer == null)
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
        if(player != null)
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

    // Método para imprimir o grafo no Console para depuração
    void PrintGraph()
    {
        graph.PrintGraph();
    }
}
