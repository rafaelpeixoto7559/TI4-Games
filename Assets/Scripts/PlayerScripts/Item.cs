using UnityEngine;

// Enum para representar as cores dos itens
public enum ItemColor
{
    Cyan,
    Green,
    Purple
}


public class Item : MonoBehaviour
{
    public ItemColor color; // Cor do item
}