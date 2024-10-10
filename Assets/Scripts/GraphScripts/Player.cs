// Player.cs
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional, se você estiver mudando de cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
