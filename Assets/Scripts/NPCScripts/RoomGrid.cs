using System.Collections.Generic;
using UnityEngine;

public class RoomGrid : MonoBehaviour
{
    public LayerMask unwalkableMask; // Máscara para identificar obstáculos
    public Vector2 gridWorldSize; // Tamanho da grade no mundo
    public float nodeRadius; // Raio de cada nó

    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        nodeDiameter = nodeRadius * 2;
        if (nodeDiameter == 0)
        {
            Debug.LogError("nodeDiameter é zero. Defina nodeRadius para um valor maior que zero.");
            return;
        }

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        if (gridSizeX <= 0 || gridSizeY <= 0)
        {
            Debug.LogError("gridSizeX ou gridSizeY é menor ou igual a zero. Verifique gridWorldSize e nodeRadius.");
            return;
        }

        grid = new Node[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Calcular a posição no mundo para este nó
                float worldX = transform.position.x - gridWorldSize.x / 2 + x * nodeDiameter + nodeRadius;
                float worldY = transform.position.y - gridWorldSize.y / 2 + y * nodeDiameter + nodeRadius;
                Vector3 worldPoint = new Vector3(worldX, worldY, 0);

                // Determinar se este nó é caminhável (não há obstáculos)
                bool walkable = !(Physics2D.OverlapCircle(worldPoint, nodeRadius, unwalkableMask));

                // Criar o nó e adicioná-lo à grade
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }



    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (grid == null)
        {
            Debug.LogError("Grid não está inicializada.");
            return null;
        }

        float percentX = (worldPosition.x - (transform.position.x - gridWorldSize.x / 2)) / gridWorldSize.x;
        float percentY = (worldPosition.y - (transform.position.y - gridWorldSize.y / 2)) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
        {
            Debug.LogError($"Índices fora dos limites: x={x}, y={y}");
            return null;
        }

        if (grid[x, y] == null)
        {
            Debug.LogError($"Nó em grid[{x}, {y}] é nulo.");
            return null;
        }

        return grid[x, y];
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        // Define os deslocamentos para cima, baixo, esquerda e direita
        int[,] directions = new int[,]
        {
            { 0, 1 },  // Cima
            { 1, 0 },  // Direita
            { 0, -1 }, // Baixo
            { -1, 0 }  // Esquerda
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int checkX = node.gridPosition.x + directions[i, 0];
            int checkY = node.gridPosition.y + directions[i, 1];

            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
