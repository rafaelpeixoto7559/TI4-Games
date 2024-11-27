using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs; // Array de prefabs de inimigos
    public int minEnemies = 1; // Mínimo de inimigos na sala
    public int maxEnemies = 5; // Máximo de inimigos na sala

    [Header("Spawn Settings")]
    public Transform spawnArea; // Área onde os inimigos podem ser spawnados
    public RoomGrid roomGrid; // Referência ao RoomGrid da sala

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool hasSpawnedEnemies = false;

    void Awake()
    {
        // Certifique-se de que o RoomGrid está configurado
        if (roomGrid == null)
        {
            roomGrid = GetComponent<RoomGrid>();
            if (roomGrid == null)
            {
                Debug.LogError("RoomController não encontrou o RoomGrid.");
            }
        }

        // Se spawnArea não estiver atribuído, tente encontrá-lo
        if (spawnArea == null)
        {
            Transform childSpawnArea = transform.Find("SpawnArea");
            if (childSpawnArea != null)
            {
                spawnArea = childSpawnArea;
            }
            else
            {
                Debug.LogError("SpawnArea não está atribuída no RoomController e não foi encontrada como filho.");
            }
        }
    }


    public void ActivateRoom()
    {
        gameObject.SetActive(true);

        if (!hasSpawnedEnemies)
        {
            SpawnEnemies();
            hasSpawnedEnemies = true;
        }

        // Ativar outros componentes, se necessário
    }

    public void DeactivateRoom()
    {
        gameObject.SetActive(false);

        // Desativar ou limpar inimigos, se necessário
    }

    void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Nenhum prefab de inimigo atribuído na sala.");
            return;
        }

        if (spawnArea == null)
        {
            Debug.LogError("SpawnArea não está atribuída no RoomController.");
            return;
        }

        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            // Calcule uma posição aleatória dentro da SpawnArea
            Vector3 spawnPosition = GetRandomPositionInArea();

            // Selecione um prefab de inimigo aleatório
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // Instancie o inimigo como filho da sala
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, transform);

            // Atribua o RoomGrid ao EnemyAI
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.roomGrid = roomGrid;
            }
            else
            {
                Debug.LogError("EnemyPrefab não tem o componente EnemyAI.");
            }

            spawnedEnemies.Add(enemy);
        }
    }

    Vector3 GetRandomPositionInArea()
    {
        // Supondo que o spawnArea tenha um Collider2D que define a área de spawn
        Collider2D collider = spawnArea.GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError("SpawnArea não possui um Collider2D.");
            return spawnArea.position;
        }

        Bounds bounds = collider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector3(x, y, spawnArea.position.z);
    }
}
