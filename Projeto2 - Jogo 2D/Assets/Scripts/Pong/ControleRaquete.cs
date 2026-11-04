
using UnityEngine;

public class ControleRaquete : MonoBehaviour
{
    [Header("Configurações do Jogador")]
    [SerializeField] private float velocidade = 8f;

    [Header("Controles")]
    [SerializeField] private KeyCode teclaCima;
    [SerializeField] private KeyCode teclaBaixo;

    [Header("Limites de Movimento")]
    [SerializeField] private float limiteSuperior = 4f;
    [SerializeField] private float limiteInferior = -4f;

    private void Update()
    {
        MovimentarJogador();
    }

    private void MovimentarJogador()
    {
        float movimento = 0f;

        if (Input.GetKey(teclaCima))
        {
            movimento = 1f;
        }
        else if (Input.GetKey(teclaBaixo))
        {
            movimento = -1f;
        }

        Vector3 direcao = Vector3.up * movimento;

        transform.position += direcao * velocidade * Time.deltaTime;

        // Impede que a raquete ultrapasse os limites.
        float posicaoY = Mathf.Clamp(
            transform.position.y,
            limiteInferior,
            limiteSuperior
        );

        transform.position = new Vector3(
            transform.position.x,
            posicaoY,
            transform.position.z
        );
    }
}
