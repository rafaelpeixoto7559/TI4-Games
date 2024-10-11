using UnityEngine;
using UnityEngine.UI;

public class SliderTextUpdater : MonoBehaviour
{
    public Slider slider; // O slider que vai controlar o volume
    public Text text;     // O texto que exibe o valor do slider

    void Start()
    {
        // Inicializa o valor do slider e o texto com o volume atual da música
        if (MusicManager.GetInstance() != null)
        {
            slider.value = MusicManager.GetInstance().GetComponent<AudioSource>().volume * 100;
        }
        
        UpdateText(slider.value); // Atualiza o texto com o valor inicial do slider

        // Adiciona um listener ao Slider para atualizar o texto e o volume da música quando o valor mudar
        slider.onValueChanged.AddListener(UpdateText);
    }

    // Método para atualizar o texto e o volume da música
    private void UpdateText(float value)
    {
        text.text = value.ToString("F0"); // Atualiza o texto com o valor do slider

        // Ajusta o volume da música baseado no valor do slider
        if (MusicManager.GetInstance() != null)
        {
            // O volume do AudioSource vai de 0 a 1, então dividimos o valor do slider (0-100) por 100
            MusicManager.GetInstance().SetVolume(value / 100f);
        }
    }
}