// Graph.cs
using System.Collections.Generic;
using UnityEngine;

// Classe representando o grafo das salas
public class Graph
{
    public int Vertices;
    public List<Edge> Edges;
    public Dictionary<int, List<int>> adjacencyList;
    public List<int> roomMaxDegrees; // Lista para armazenar o grau máximo de cada sala

    private System.Random random;

    // Construtor modificado para inicializar roomMaxDegrees
    public Graph(int vertices)
    {
        Vertices = vertices;
        Edges = new List<Edge>();
        adjacencyList = new Dictionary<int, List<int>>();
        roomMaxDegrees = new List<int>(); // Inicializa a lista de graus máximos
        random = new System.Random();

        for (int i = 0; i < vertices; i++)
        {
            adjacencyList[i] = new List<int>();
            roomMaxDegrees.Add(GetMaxDegree(i)); // Define o grau máximo baseado no índice da sala
        }
    }

    // Método para definir o grau máximo com base no índice da sala
    private int GetMaxDegree(int roomIndex)
    {
        // Supondo que:
        // Salas 0 a 3 têm 1 porta
        // Salas 4 a 6 têm 2 portas
        // Salas 7 a 9 têm 3 portas
        if (roomIndex < 4)
            return 1;
        else if (roomIndex < 7)
            return 2;
        else
            return 3;
    }

    // Método para gerar um grafo conexo utilizando o Algoritmo de Krusky com restrição de grau por sala
    public void GenerateConnectedGraph()
    {
        List<Edge> allPossibleEdges = new List<Edge>();

        // Gerar todas as possíveis arestas sem duplicatas e sem laços
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
        int maxAttempts = 1000;
        int attempts = 0;

        foreach (var edge in allPossibleEdges)
        {
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Número máximo de tentativas alcançado durante a geração do grafo.");
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
            }

            attempts++;
        }

        // Verificar se o grafo é conectado
        if (!IsConnected())
        {
            Debug.LogError("O grafo gerado não está conectado. Tente gerar novamente.");
            // Opcional: Implementar lógica para reconectar componentes desconexos
        }
        else
        {
            Debug.Log("Grafo gerado com sucesso e é conexo.");
        }
    }

    // Adiciona uma aresta ao grafo
    private void AddEdge(int source, int destination)
    {
        Edge newEdge = new Edge(source, destination);
        Edges.Add(newEdge);
        adjacencyList[source].Add(destination);
        adjacencyList[destination].Add(source);
        Debug.Log($"Adicionada aresta: {source} -- {destination}");
    }

    // Método para verificar se o grafo é conexo
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

    // Busca em profundidade para verificar conectividade
    private void DFS(int vertex, bool[] visited)
    {
        visited[vertex] = true;

        foreach (int neighbor in adjacencyList[vertex])
        {
            if (!visited[neighbor])
                DFS(neighbor, visited);
        }
    }

    // Embaralha a lista de arestas utilizando o algoritmo de Fisher-Yates
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

    // Apenas para depuração, imprime todas as arestas do grafo
    public void PrintGraph()
    {
        Debug.Log("Arestas do Grafo:");
        foreach (var edge in Edges)
        {
            Debug.Log($"{edge.Source} -- {edge.Destination}");
        }
    }
}
