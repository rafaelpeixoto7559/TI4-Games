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

    [Header("Player Settings")]
    public GameObject playerPrefab; // Prefab do jogador
    public Vector3 playerSpawnOffset = new Vector3(0, -2, 0); // Offset para spawn do jogador dentro da sala

    [Header("Spawn Settings")]
    public Vector3 roomSpawnBasePosition = Vector3.zero; // Posição base para spawn das salas
    public float roomSpacing = 20f; // Espaçamento entre as salas

    [Header("Graph Settings")]
    public int numberOfRooms = 3;  // Número total de salas (alterado para 3 conforme seu exemplo)
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

        // Obter a representação DOT como string
        string dot = graph.ToDotFormat();
        Debug.Log("Representação DOT do Grafo:\n" + dot);

        // Opcional: Exportar para um arquivo DOT
        string caminhoDoArquivo = Path.Combine(Application.dataPath, "grafo.dot");
        graph.ExportToDotFile(caminhoDoArquivo);

        // Instanciar as salas com base no grafo gerado e rotacioná-las
        GenerateRooms();

        // Conectar as salas com portas
        ConnectRooms();

        // Verificar se todas as portas estão alinhadas corretamente
        if (VerifyAllDoorsAligned())
        {
            Debug.Log("Todas as portas estão alinhadas corretamente.");
        }
        else
        {
            Debug.LogError("Há portas desalinhadas nas salas. Verifique a lógica de rotacionamento.");
        }

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

            // Instancia a sala
            GameObject room = Instantiate(roomPrefabToUse, roomSpawnBasePosition, Quaternion.identity);
            room.name = "Room " + i;
            room.SetActive(false);  // Desativa a sala inicialmente
            rooms.Add(room);

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

                // Determinar a direção correta para rotacionar as salas
                DoorDirection directionAOriginal = GetOriginalDoorDirection(edge.Source, roomDoorCounters[edge.Source]);
                DoorDirection directionBOriginal = GetOriginalDoorDirection(edge.Destination, roomDoorCounters[edge.Destination]);

                // Calcular a rotação necessária para alinhar as portas
                // A porta da sala A deve estar na direção oposta à da sala B
                int rotationA = CalculateRotation(directionAOriginal, DoorDirection.East); // Escolha uma direção base para Sala A
                int rotationB = CalculateRotation(directionBOriginal, DirectionHelper.GetOppositeDirection(DoorDirection.East));

                // Aplicar rotações
                RotateRoom(edge.Source, rotationA);
                RotateRoom(edge.Destination, rotationB);

                // Atribuir o RoomManager às portas agora rotacionadas
                doorA.roomManager = this;
                doorB.roomManager = this;

                // Definir a sala conectada nas portas
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

    /// <summary>
    /// Obtém a direção original da porta baseado no índice da porta na sala.
    /// </summary>
    /// <param name="roomIndex">Índice da sala.</param>
    /// <param name="doorIndex">Índice da porta na sala.</param>
    /// <returns>Direção original da porta.</returns>
    private DoorDirection GetOriginalDoorDirection(int roomIndex, int doorIndex)
    {
        // Sala1: 1 porta (East)
        // Sala2: 2 portas (East, West)
        // Sala3: 3 portas (East, West, South)

        if (graph.adjacencyList[roomIndex].Count == 1)
        {
            return DoorDirection.East;
        }
        else if (graph.adjacencyList[roomIndex].Count == 2)
        {
            if (doorIndex == 0)
                return DoorDirection.East;
            else
                return DoorDirection.West;
        }
        else if (graph.adjacencyList[roomIndex].Count == 3)
        {
            if (doorIndex == 0)
                return DoorDirection.East;
            else if (doorIndex == 1)
                return DoorDirection.West;
            else
                return DoorDirection.South;
        }
        else
        {
            return DoorDirection.North; // Default ou para casos não previstos
        }
    }

    /// <summary>
    /// Calcula a rotação necessária para alinhar uma direção base com uma direção alvo.
    /// </summary>
    /// <param name="baseDirection">Direção original.</param>
    /// <param name="targetDirection">Direção alvo após rotação.</param>
    /// <returns>Número de rotações de 90 graus necessárias.</returns>
    private int CalculateRotation(DoorDirection baseDirection, DoorDirection targetDirection)
    {
        int rotationSteps = 0;
        DoorDirection currentDirection = baseDirection;

        while (currentDirection != targetDirection && rotationSteps < 4)
        {
            currentDirection = DirectionHelper.RotateDirection(currentDirection, 1);
            rotationSteps++;
        }

        return rotationSteps % 4;
    }

    /// <summary>
    /// Rotaciona uma sala em passos de 90 graus.
    /// </summary>
    /// <param name="roomIndex">Índice da sala a ser rotacionada.</param>
    /// <param name="rotationSteps">Número de rotações de 90 graus (0-3).</param>
    private void RotateRoom(int roomIndex, int rotationSteps)
    {
        GameObject room = rooms[roomIndex];
        room.transform.Rotate(0, 0, rotationSteps * 90);
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

    // Método para verificar se todas as portas estão alinhadas corretamente
    bool VerifyAllDoorsAligned()
    {
        bool allAligned = true;

        foreach(var edge in graph.Edges)
        {
            int roomAIndex = edge.Source;
            int roomBIndex = edge.Destination;

            DoorTrigger doorA = rooms[roomAIndex].GetComponentsInChildren<DoorTrigger>()[roomDoorCounters[roomAIndex]-1];
            DoorTrigger doorB = rooms[roomBIndex].GetComponentsInChildren<DoorTrigger>()[roomDoorCounters[roomBIndex]-1];

            // Obter a direção atual das portas após rotação
            DoorDirection directionA = GetCurrentDoorDirection(roomAIndex, roomDoorCounters[roomAIndex]-1);
            DoorDirection directionB = GetCurrentDoorDirection(roomBIndex, roomDoorCounters[roomBIndex]-1);

            // Verificar se as direções são opostas
            if(directionA != DirectionHelper.GetOppositeDirection(directionB))
            {
                Debug.LogError($"Portas entre Sala {roomAIndex} e Sala {roomBIndex} não estão alinhadas.");
                allAligned = false;
            }
        }

        if(!allAligned)
        {
            Debug.LogError("Há portas desalinhadas nas salas. Verifique a lógica de rotacionamento.");
        }

        return allAligned;
    }

    /// <summary>
    /// Obtém a direção atual da porta baseado na rotação da sala.
    /// </summary>
    /// <param name="roomIndex">Índice da sala.</param>
    /// <param name="doorIndex">Índice da porta na sala.</param>
    /// <returns>Direção atual da porta após rotação.</returns>
    private DoorDirection GetCurrentDoorDirection(int roomIndex, int doorIndex)
    {
        DoorDirection originalDirection = GetOriginalDoorDirection(roomIndex, doorIndex);
        int rotationSteps = graph.roomRotations[roomIndex];
        return DirectionHelper.RotateDirection(originalDirection, rotationSteps);
    }

    // Método para imprimir o grafo no Console para depuração
    void PrintGraph()
    {
        graph.PrintGraph();
    }
}
