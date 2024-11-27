// RoomState.cs

// Adicione estas diretivas no topo do arquivo
using UnityEngine;  // Necessário para 'GameObject'
using System.Collections.Generic;  // Necessário para 'List<>'

public class RoomState
{
    public GameObject roomInstance;
    public bool isVisited;
    public List<GameObject> spawnedEnemies;

    public RoomState(GameObject roomInstance)
    {
        this.roomInstance = roomInstance;
        isVisited = false;
        spawnedEnemies = new List<GameObject>();
    }
}
