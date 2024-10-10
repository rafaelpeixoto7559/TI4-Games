using UnityEngine;
using UnityEngine.UI;

public class SliderTextUpdater : MonoBehaviour
{
    public Slider slider; 
    public Text text;    

    void Start()
    {
        // Atualiza o texto inicialmente
        UpdateText(50);

        // Adiciona um listener ao Slider para atualizar o texto quando o valor mudar
        slider.onValueChanged.AddListener(UpdateText);
    }

    // Método para atualizar o texto com o valor do Slider
    private void UpdateText(float value)
    {
        text.text = value.ToString("F0");
    }
}