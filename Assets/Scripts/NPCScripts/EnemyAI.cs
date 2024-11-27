using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Transform target; // Referência ao jogador
    private List<Node> path;
    private int currentPathIndex;
    public RoomGrid roomGrid; // Variável pública para receber o RoomGrid
    public float speed = 5f;
    public float pathUpdateInterval = 0.2f; // Intervalo para atualizar o caminho

    private float pathUpdateTimer = 0f;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        if (roomGrid == null)
        {
            Debug.LogError("EnemyAI não tem uma referência ao RoomGrid.");
            enabled = false;
            return;
        }

        FindPath();
    }

    void Update()
    {
        if (target == null || roomGrid == null)
            return;

        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateInterval)
        {
            pathUpdateTimer = 0f;
            FindPath();
        }

        MoveAlongPath();
    }


    void FindPath()
    {
        if (roomGrid == null)
            return;

        roomGrid.ResetNodes();

        Node startNode = roomGrid.NodeFromWorldPoint(transform.position);
        Node targetNode = roomGrid.NodeFromWorldPoint(target.position);

        if (!startNode.walkable || !targetNode.walkable)
        {
            if (!startNode.walkable)
            {
                Debug.LogError("Nó inicial não é caminhável.");
            }

            if (!targetNode.walkable)
            {
                Debug.LogError("Nó de destino não é caminhável.");
            }
            return;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Ordena o openSet com base no fCost
            openSet.Sort((nodeA, nodeB) => nodeA.fCost.CompareTo(nodeB.fCost));

            Node currentNode = openSet[0];
            openSet.RemoveAt(0);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in roomGrid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int tentativeGCost = currentNode.gCost + GetDistance(currentNode, neighbour);

                if (tentativeGCost < neighbour.gCost)
                {
                    neighbour.gCost = tentativeGCost;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }


    void RetracePath(Node startNode, Node endNode)
    {
        path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        currentPathIndex = 0;
    }

    void MoveAlongPath()
    {
        if (path == null || currentPathIndex >= path.Count)
            return;

        Vector3 targetPosition = path[currentPathIndex].worldPosition;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
        }
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        return 10 * (dstX + dstY); // Usando distância Manhattan
    }

}
