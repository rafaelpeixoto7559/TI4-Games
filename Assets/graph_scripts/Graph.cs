// Graph.cs
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

public enum RoomType
{
    Sala1, // 1 porta
    Sala2, // 2 portas
    Sala3  // 3 portas
}

public class Graph
{
    public int Vertices;
    public List<Edge> Edges;
    public Dictionary<int, List<int>> adjacencyList;
    public List<int> roomMaxDegrees; // Lista para armazenar o grau máximo de cada sala

    private System.Random random;

    // Rotação atribuída a cada sala (em passos de 90°: 0, 1, 2, 3 correspondem a 0°, 90°, 180°, 270°)
    public int[] roomRotations;

    // Tipos de sala
    public RoomType[] roomTypes;

    // Tracking used doors per room
    private List<DoorDirection>[] roomUsedDoors;

    public Graph(int vertices)
    {
        Vertices = vertices;
        Edges = new List<Edge>();
        adjacencyList = new Dictionary<int, List<int>>();
        roomMaxDegrees = new List<int>();
        random = new System.Random();
        roomRotations = new int[vertices];
        roomTypes = new RoomType[vertices];
        roomUsedDoors = new List<DoorDirection>[vertices];

        for (int i = 0; i < vertices; i++)
        {
            adjacencyList[i] = new List<int>();
            roomUsedDoors[i] = new List<DoorDirection>();

            // Assign room type based on index, por exemplo:
            // Cycle through Sala1, Sala2, Sala3
            if (i % 3 == 0)
            {
                roomTypes[i] = RoomType.Sala1;
                roomMaxDegrees.Add(1);
            }
            else if (i % 3 == 1)
            {
                roomTypes[i] = RoomType.Sala2;
                roomMaxDegrees.Add(2);
            }
            else
            {
                roomTypes[i] = RoomType.Sala3;
                roomMaxDegrees.Add(3);
            }

            roomRotations[i] = -1; // -1 indica que a rotação ainda não foi atribuída
        }
    }

    /// <summary>
    /// Gera uma árvore geradora mínima conectada com restrição de grau máximo baseado no tipo de sala.
    /// </summary>
    public void GenerateConnectedGraph()
    {
        int maxRetries = 100; // Número máximo de tentativas para gerar uma árvore geradora válida
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            // Limpar o grafo atual
            Edges.Clear();
            foreach (var key in adjacencyList.Keys)
            {
                adjacencyList[key].Clear();
            }

        for (int i = 0; i < roomUsedDoors.Length; i++)
        {
            roomUsedDoors[i].Clear();
        }

            // Gerar todas as possíveis arestas sem duplicatas e sem laços
            List<Edge> allPossibleEdges = new List<Edge>();
            for (int i = 0; i < Vertices; i++)
            {
                for (int j = i + 1; j < Vertices; j++)
                {
                    allPossibleEdges.Add(new Edge(i, j));
                }
            }

            // Embaralhar as arestas para randomização
            Shuffle(allPossibleEdges);

            UnionFind uf = new UnionFind(Vertices);
            List<Edge> spanningTree = new List<Edge>();
            int attemptsCount = 0;
            int maxAttempts = 1000;

            // Primeira etapa: Construir a árvore geradora
            foreach (var edge in allPossibleEdges)
            {
                if (spanningTree.Count == Vertices - 1)
                    break;

                if (attemptsCount >= maxAttempts)
                {
                    Debug.LogWarning($"Tentativa {attempt + 1}: Número máximo de tentativas alcançado durante a construção da árvore geradora.");
                    break;
                }

                int src = edge.Source;
                int dest = edge.Destination;

                // Verifica se adicionando a aresta não cria um ciclo e se ambos os vértices têm grau disponível
                if (uf.Find(src) != uf.Find(dest) &&
                    adjacencyList[src].Count < roomMaxDegrees[src] &&
                    adjacencyList[dest].Count < roomMaxDegrees[dest])
                {
                    uf.Union(src, dest);
                    AddEdge(edge.Source, edge.Destination);
                    spanningTree.Add(edge);
                }

                attemptsCount++;
            }

            // Verificar se a árvore geradora conecta todos os vértices
            if (spanningTree.Count == Vertices - 1 && IsConnected())
            {
                Debug.Log($"Árvore geradora conectada gerada com sucesso na tentativa {attempt + 1}.");

                // Agora, tentar atribuir rotações
                if (AssignRotations())
                {
                    Debug.Log("Rotações atribuídas com sucesso.");
                    return;
                }
                else
                {
                    Debug.LogWarning($"Tentativa {attempt + 1}: Falha ao atribuir rotações. Tentando novamente...");
                }
            }
            else
            {
                Debug.LogWarning($"Tentativa {attempt + 1}: Falha ao gerar uma árvore geradora conectada válida. Tentando novamente...");
            }
        }

        // Se todas as tentativas falharem
        Debug.LogError("Falha ao gerar uma árvore geradora conectada após várias tentativas. Verifique as restrições de grau ou o número de vértices.");
    }

    /// <summary>
    /// Atribui rotações às salas garantindo que as portas conectadas estejam alinhadas.
    /// </summary>
    /// <returns>True se as rotações puderem ser atribuídas corretamente, False caso contrário.</returns>
    private bool AssignRotations()
    {
        // Inicializar todas as rotações como não atribuídas (-1)
        for (int i = 0; i < Vertices; i++)
        {
            roomRotations[i] = -1;
            roomUsedDoors[i].Clear();
        }

        // Escolher a sala inicial (pode ser a primeira sala)
        int initialRoom = 0;
        roomRotations[initialRoom] = 0; // Rotação 0°

        // Iniciar o backtracking a partir da sala inicial
        return BacktrackAssignRotations(initialRoom);
    }

    /// <summary>
    /// Função recursiva de backtracking para atribuir rotações às salas.
    /// </summary>
    /// <param name="currentRoom">Sala atual sendo processada.</param>
    /// <returns>True se uma atribuição válida for encontrada, False caso contrário.</returns>
    private bool BacktrackAssignRotations(int currentRoom)
    {
        // Iterar sobre todas as salas conectadas à sala atual
        foreach (int connectedRoom in adjacencyList[currentRoom])
        {
            if (roomRotations[connectedRoom] == -1)
            {
                // A sala conectada ainda não tem rotação atribuída
                // Precisamos determinar a rotação que alinha a porta de 'currentRoom' com a porta de 'connectedRoom'

                // Identificar a direção da porta em 'currentRoom' que conecta a 'connectedRoom'
                DoorDirection directionInCurrent = GetConnectingDoorDirection(currentRoom, connectedRoom);

                // A direção na 'connectedRoom' deve ser oposta
                DoorDirection requiredDirectionInConnected = DirectionHelper.GetOppositeDirection(directionInCurrent);

                // Tentar todas as possíveis rotações para 'connectedRoom' até encontrar uma que satisfaça a condição
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    // Verificar se a 'connectedRoom' tem uma porta na direção requerida após a rotação
                    bool hasRequiredDoor = HasDoorInDirection(connectedRoom, requiredDirectionInConnected, rotation);

                    if (hasRequiredDoor && roomUsedDoors[connectedRoom].Count < roomMaxDegrees[connectedRoom])
                    {
                        // Atribuir a rotação
                        roomRotations[connectedRoom] = rotation;

                        // Marcar a porta usada na connectedRoom
                        roomUsedDoors[connectedRoom].Add(requiredDirectionInConnected);

                        // Marcar a porta usada na currentRoom
                        roomUsedDoors[currentRoom].Add(directionInCurrent);

                        // Verificar se esta atribuição não conflita com rotações previamente atribuídas
                        if (IsValidRotation(currentRoom))
                        {
                            // Recursivamente atribuir rotações para as salas conectadas à 'connectedRoom'
                            if (BacktrackAssignRotations(connectedRoom))
                            {
                                return true; // Sucesso
                            }
                        }

                        // Se falhar, remover a atribuição e tentar a próxima rotação
                        roomRotations[connectedRoom] = -1;
                        roomUsedDoors[connectedRoom].Remove(requiredDirectionInConnected);
                        roomUsedDoors[currentRoom].Remove(directionInCurrent);
                    }
                }

                // Se nenhuma rotação for válida para a 'connectedRoom', retroceder
                return false;
            }
            else
            {
                // A sala conectada já tem rotação atribuída
                // Verificar se as portas estão alinhadas corretamente
                DoorDirection directionInCurrent = GetConnectingDoorDirection(currentRoom, connectedRoom);
                DoorDirection requiredDirectionInConnected = DirectionHelper.GetOppositeDirection(directionInCurrent);

                // A rotação atribuída deve ter uma porta na direção requerida
                bool isAligned = HasDoorInDirection(connectedRoom, requiredDirectionInConnected, roomRotations[connectedRoom]);

                if (!isAligned)
                {
                    return false; // Conflito encontrado
                }
                else
                {
                    // Marcar as portas usadas
                    roomUsedDoors[currentRoom].Add(directionInCurrent);
                    roomUsedDoors[connectedRoom].Add(requiredDirectionInConnected);
                }
            }
        }

        // Todas as conexões da sala atual foram processadas sem conflitos
        return true;
    }

    /// <summary>
    /// Obtém a direção da porta na 'currentRoom' que conecta à 'connectedRoom'.
    /// </summary>
    /// <param name="currentRoom">Sala atual.</param>
    /// <param name="connectedRoom">Sala conectada.</param>
    /// <returns>Direção da porta na 'currentRoom'.</returns>
    private DoorDirection GetConnectingDoorDirection(int currentRoom, int connectedRoom)
    {
        // Identificar a direção da porta na 'currentRoom' que conecta à 'connectedRoom'
        // Para cada porta da sala atual, verificar se está conectada à 'connectedRoom'

        // Obter as portas rotacionadas
        List<DoorDirection> rotatedDoors = RotateOriginalDoors(currentRoom, roomRotations[currentRoom]);

        // Iterar pelas portas rotacionadas e verificar se alguma pode ser usada para conectar à 'connectedRoom'
        for (int i = 0; i < rotatedDoors.Count; i++)
        {
            DoorDirection door = rotatedDoors[i];
            // A direção oposta deve ter uma porta na 'connectedRoom'
            DoorDirection oppositeDoor = DirectionHelper.GetOppositeDirection(door);
            if (HasDoorInDirection(connectedRoom, oppositeDoor, roomRotations[connectedRoom]))
            {
                // Verificar se a porta está disponível (não usada)
                if (!roomUsedDoors[currentRoom].Contains(door))
                {
                    return door;
                }
            }
        }

        // Se nenhuma porta for encontrada, retornar uma direção padrão (não ideal)
        Debug.LogWarning($"Sala {currentRoom} não conseguiu encontrar uma porta válida para conectar à Sala {connectedRoom}");
        return DoorDirection.North; // Default
    }

    /// <summary>
    /// Rotaciona as portas originais de uma sala com base na rotação.
    /// </summary>
    /// <param name="roomIndex">Índice da sala.</param>
    /// <param name="rotation">Rotação aplicada (0-3, correspondendo a 0°, 90°, 180°, 270°).</param>
    /// <returns>Lista de direções das portas após rotação.</returns>
    private List<DoorDirection> RotateOriginalDoors(int roomIndex, int rotation)
    {
        List<DoorDirection> originalDoors = GetOriginalDoors(roomIndex);
        List<DoorDirection> rotatedDoors = new List<DoorDirection>();
        foreach (var door in originalDoors)
        {
            rotatedDoors.Add(DirectionHelper.RotateDirection(door, rotation));
        }
        return rotatedDoors;
    }

    /// <summary>
    /// Verifica se uma sala possui uma porta na direção especificada após uma rotação.
    /// </summary>
    /// <param name="roomIndex">Índice da sala.</param>
    /// <param name="direction">Direção requerida.</param>
    /// <param name="rotation">Rotação aplicada (0-3, correspondendo a 0°, 90°, 180°, 270°).</param>
    /// <returns>True se a sala possui uma porta na direção após a rotação, False caso contrário.</returns>
    private bool HasDoorInDirection(int roomIndex, DoorDirection direction, int rotation)
    {
        // Obter as portas originais da sala
        List<DoorDirection> originalDoors = GetOriginalDoors(roomIndex);

        // Rotacionar as portas
        List<DoorDirection> rotatedDoors = new List<DoorDirection>();
        foreach (var door in originalDoors)
        {
            rotatedDoors.Add(DirectionHelper.RotateDirection(door, rotation));
        }

        // Verificar se a direção requerida está presente e não está sendo usada
        return rotatedDoors.Contains(direction);
    }

    /// <summary>
    /// Obtém as portas originais de uma sala sem rotação.
    /// </summary>
    /// <param name="roomIndex">Índice da sala.</param>
    /// <returns>Lista de direções das portas.</returns>
    private List<DoorDirection> GetOriginalDoors(int roomIndex)
    {
        // Sala1: 1 porta (East)
        // Sala2: 2 portas (East, West)
        // Sala3: 3 portas (East, West, South)

        if (roomTypes[roomIndex] == RoomType.Sala1)
        {
            return new List<DoorDirection> { DoorDirection.East };
        }
        else if (roomTypes[roomIndex] == RoomType.Sala2)
        {
            return new List<DoorDirection> { DoorDirection.East, DoorDirection.West };
        }
        else if (roomTypes[roomIndex] == RoomType.Sala3)
        {
            return new List<DoorDirection> { DoorDirection.East, DoorDirection.West, DoorDirection.South };
        }
        else
        {
            return new List<DoorDirection>(); // Caso não haja portas
        }
    }

    /// <summary>
    /// Verifica se a rotação atribuída a uma sala não causa conflitos com rotações de salas conectadas.
    /// </summary>
    /// <param name="roomIndex">Índice da sala.</param>
    /// <returns>True se a rotação for válida, False caso contrário.</returns>
    private bool IsValidRotation(int roomIndex)
    {
        // Para cada conexão da sala, verificar se as portas estão alinhadas
        foreach (int connectedRoom in adjacencyList[roomIndex])
        {
            if (roomRotations[connectedRoom] == -1)
                continue; // Ainda não foi atribuído

            // Obter a direção da porta na sala atual que conecta à sala conectada
            DoorDirection directionInCurrent = GetConnectingDoorDirection(roomIndex, connectedRoom);
            // A direção requerida na sala conectada
            DoorDirection requiredDirectionInConnected = DirectionHelper.GetOppositeDirection(directionInCurrent);

            // Verificar se a sala conectada possui uma porta nessa direção após sua rotação
            bool isAligned = HasDoorInDirection(connectedRoom, requiredDirectionInConnected, roomRotations[connectedRoom]);

            if (!isAligned)
                return false; // Conflito encontrado
        }

        return true; // Nenhum conflito
    }

    /// <summary>
    /// Adiciona uma aresta ao grafo.
    /// </summary>
    /// <param name="source">Índice da sala de origem.</param>
    /// <param name="destination">Índice da sala de destino.</param>
    private void AddEdge(int source, int destination)
    {
        Edge newEdge = new Edge(source, destination);
        Edges.Add(newEdge);
        adjacencyList[source].Add(destination);
        adjacencyList[destination].Add(source);
        Debug.Log($"Adicionada aresta: {source} -- {destination}");
    }

    /// <summary>
    /// Método para verificar se o grafo é conexo.
    /// </summary>
    public bool IsConnected()
    {
        bool[] visited = new bool[Vertices];
        DFS(0, visited);

        for (int i = 0; i < Vertices; i++)
        {
            if (!visited[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Busca em profundidade para verificar conectividade.
    /// </summary>
    /// <param name="vertex">Vértice atual.</param>
    /// <param name="visited">Array de vértices visitados.</param>
    private void DFS(int vertex, bool[] visited)
    {
        visited[vertex] = true;

        foreach (int neighbor in adjacencyList[vertex])
        {
            if (!visited[neighbor])
                DFS(neighbor, visited);
        }
    }

    /// <summary>
    /// Embaralha a lista de arestas utilizando o algoritmo de Fisher-Yates.
    /// </summary>
    /// <param name="list">Lista de arestas.</param>
    private void Shuffle(List<Edge> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            Edge value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Apenas para depuração, imprime todas as arestas do grafo.
    /// </summary>
    public void PrintGraph()
    {
        Debug.Log("Arestas do Grafo:");
        foreach (var edge in Edges)
        {
            Debug.Log($"{edge.Source} -- {edge.Destination}");
        }
    }

    /// <summary>
    /// Gera uma representação do grafo no formato DOT.
    /// </summary>
    /// <returns>String no formato DOT representando o grafo.</returns>
    public string ToDotFormat()
    {
        StringBuilder sb = new StringBuilder();

        // Início da definição do grafo. Usamos "graph" para grafos não direcionados.
        sb.AppendLine("graph G {");

        // Opcional: Definir atributos globais
        sb.AppendLine("    node [shape=circle];"); // Define o formato dos nós

        // Adicionar todas as arestas
        foreach (var edge in Edges)
        {
            sb.AppendLine($"    {edge.Source} -- {edge.Destination};");
        }

        sb.AppendLine("}"); // Fim da definição do grafo

        return sb.ToString();
    }

    /// <summary>
    /// Exporta a representação DOT do grafo para um arquivo especificado.
    /// </summary>
    /// <param name="filePath">Caminho completo para o arquivo de saída.</param>
    public void ExportToDotFile(string filePath)
    {
        string dotFormat = ToDotFormat();
        File.WriteAllText(filePath, dotFormat);
        Debug.Log($"Grafo exportado para o arquivo DOT: {filePath}");
    }

    /// <summary>
    /// Verifica se todas as portas estão alinhadas corretamente.
    /// </summary>
    public bool VerifyAllDoorsAligned()
    {
        bool allAligned = true;
        foreach(var edge in Edges)
        {
            int roomA = edge.Source;
            int roomB = edge.Destination;

            DoorDirection directionA = GetConnectingDoorDirection(roomA, roomB);
            DoorDirection directionB = DirectionHelper.GetOppositeDirection(directionA);

            bool roomBHasDirection = HasDoorInDirection(roomB, directionB, roomRotations[roomB]);

            if (!roomBHasDirection)
            {
                Debug.LogError($"Portas entre Sala {roomA} e Sala {roomB} não estão alinhadas.");
                allAligned = false;
            }
        }

        if (!allAligned)
        {
            Debug.LogError("Há portas desalinhadas nas salas. Verifique a lógica de rotacionamento.");
        }

        return allAligned;
    }
}
