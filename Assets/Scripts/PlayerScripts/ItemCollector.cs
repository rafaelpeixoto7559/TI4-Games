using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    // Contadores para os itens
    private int cyanCount = 0;
    private int greenCount = 0;
    private int purpleCount = 0;

    void UpdateShopCounts()
    {
        if (SkillShop.Instance != null)
        {
            SkillShop.Instance.SetCounts(cyanCount, greenCount, purpleCount);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            // Obtém o componente do item coletado
            Item item = other.GetComponent<Item>();

            if (item != null)
            {
                // Incrementa o contador correspondente e atualiza o texto
                if (item.color == ItemColor.Cyan)
                {
                    cyanCount++;
                    UIManager.Instance.cyanText.text = cyanCount.ToString(); // Atualiza apenas o número
                    UpdateShopCounts();
                }
                else if (item.color == ItemColor.Green)
                {
                    greenCount++;
                    UIManager.Instance.greenText.text = greenCount.ToString(); // Atualiza apenas o número
                    UpdateShopCounts();
                }
                else if (item.color == ItemColor.Purple)
                {
                    purpleCount++;
                    UIManager.Instance.purpleText.text = purpleCount.ToString(); // Atualiza apenas o número
                    UpdateShopCounts();
                }

                Destroy(other.gameObject); // Destroi o item
            }
        }
    }

    private void Start()
    {
        // Garante que os textos comecem com "0"
        UIManager.Instance.cyanText.text = "0";
        UIManager.Instance.greenText.text = "0";
        UIManager.Instance.purpleText.text = "0";
    }
}