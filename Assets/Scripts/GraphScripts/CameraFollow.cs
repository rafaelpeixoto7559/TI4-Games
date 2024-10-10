using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // O alvo que a câmera deve seguir (Jogador)
    public Vector3 offset = new Vector3(0, 0, -10); // Offset da câmera em relação ao alvo
    public float smoothSpeed = 5f;    // Velocidade de suavização

    void LateUpdate()
    {
        if (target != null)
        {
            // Calcula a posição desejada da câmera
            Vector3 desiredPosition = target.position + offset;
            // Suaviza o movimento da câmera
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            // Atualiza a posição da câmera
            transform.position = smoothedPosition;
        }
    }

    // Método para definir o alvo da câmera
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
