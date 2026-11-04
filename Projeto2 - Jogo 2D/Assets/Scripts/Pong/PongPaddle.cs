
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PongPaddle : MonoBehaviour
{
    [Header("Movimentacao")]
    [SerializeField] private float velocidadeMovimento = 8f;
    [SerializeField] private float limiteY = 4f;

    [Header("Inteligencia Artificial")]
    [SerializeField] private float velocidadeIA = 6.5f;
    [SerializeField] private float margemErroIA = 0.15f;

    [Header("Energia Especial")]
    [SerializeField] private float tempoCargaCompleta = 8f;
    [SerializeField] private float multiplicadorChute = 1.8f;
    [SerializeField] private Color corCarregada = Color.cyan;

    [Header("Barra Interna")]
    [Range(0.1f, 0.8f)]
    [SerializeField] private float larguraRelativa = 0.38f;
    [Range(0.5f, 1f)]
    [SerializeField] private float alturaRelativa = 0.82f;
    [SerializeField] private bool mostrarBarraEnergia = true;

    [Header("Estado")]
    [SerializeField] private bool controladoPorIA;

    private Rigidbody2D rb;
    private PongBall bola;

    private KeyCode teclaCima = KeyCode.W;
    private KeyCode teclaBaixo = KeyCode.S;
    private KeyCode teclaEnergia = KeyCode.LeftShift;

    private float direcao;
    private float energia;

    private bool partidaAtiva;
    private bool energiaArmada;

    private SpriteRenderer fundoBarra;
    private SpriteRenderer preenchimentoBarra;
    private SpriteRenderer brilhoBarra;

    private static Sprite spritePixel;

    public Color CorEnergia => corCarregada;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CriarBarraInterna();
    }

    private void Start()
    {
        if (bola == null)
            bola = FindFirstObjectByType<PongBall>();

        AtualizarVisualEnergia();
    }

    private void Update()
    {
        if (!partidaAtiva)
        {
            direcao = 0f;
            return;
        }

        if (controladoPorIA)
            ControlarIA();
        else
            ControlarHumano();

        AtualizarEnergia();
        AtualizarVisualEnergia();
    }

    private void FixedUpdate()
    {
        if (!partidaAtiva)
            return;

        float velocidade = controladoPorIA
            ? velocidadeIA
            : velocidadeMovimento;

        float novaY = rb.position.y +
            direcao * velocidade * Time.fixedDeltaTime;

        novaY = Mathf.Clamp(novaY, -limiteY, limiteY);

        rb.MovePosition(new Vector2(rb.position.x, novaY));
    }

    // CONTROLE HUMANO

    private void ControlarHumano()
    {
        direcao = 0f;

        if (Input.GetKey(teclaCima))
            direcao += 1f;

        if (Input.GetKey(teclaBaixo))
            direcao -= 1f;

        if (teclaEnergia != KeyCode.None &&
            Input.GetKeyDown(teclaEnergia))
        {
            ArmarChuteEspecial();
        }
    }

    // INTELIGENCIA ARTIFICIAL

    private void ControlarIA()
    {
        if (bola == null)
        {
            direcao = 0f;
            return;
        }

        float diferenca =
            bola.transform.position.y - rb.position.y;

        if (Mathf.Abs(diferenca) <= margemErroIA)
            direcao = 0f;
        else
            direcao = Mathf.Sign(diferenca);

        if (energia >= 1f && !energiaArmada)
            energiaArmada = true;
    }

    // CRIACAO AUTOMATICA DA BARRA

    private void CriarBarraInterna()
    {
        if (!mostrarBarraEnergia)
            return;

        SpriteRenderer jogador = GetComponent<SpriteRenderer>();

        Vector2 tamanho = jogador != null && jogador.sprite != null
            ? jogador.sprite.bounds.size
            : new Vector2(0.5f, 2f);

        float largura = tamanho.x * larguraRelativa;
        float altura = tamanho.y * alturaRelativa;

        fundoBarra = CriarElementoBarra(
            "BarraEnergia_Fundo",
            new Color(0.015f, 0.02f, 0.08f, 0.95f),
            largura,
            altura,
            1
        );

        brilhoBarra = CriarElementoBarra(
            "BarraEnergia_Brilho",
            new Color(corCarregada.r, corCarregada.g,
                corCarregada.b, 0f),
            largura * 1.5f,
            altura,
            2
        );

        preenchimentoBarra = CriarElementoBarra(
            "BarraEnergia_Preenchimento",
            corCarregada,
            largura * 0.72f,
            0f,
            3
        );

        fundoBarra.transform.localPosition = Vector3.zero;
    }

    private SpriteRenderer CriarElementoBarra(
        string nome,
        Color cor,
        float largura,
        float altura,
        int ordem)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(transform, false);

        SpriteRenderer sr = objeto.AddComponent<SpriteRenderer>();

        if (spritePixel == null)
        {
            Texture2D textura = new Texture2D(
                1, 1, TextureFormat.RGBA32, false
            );

            textura.SetPixel(0, 0, Color.white);
            textura.Apply();

            spritePixel = Sprite.Create(
                textura,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1f
            );
        }

        sr.sprite = spritePixel;
        sr.color = cor;
        sr.sortingLayerID = GetComponent<SpriteRenderer>() != null
            ? GetComponent<SpriteRenderer>().sortingLayerID
            : 0;

        sr.sortingOrder = ordem;

        objeto.transform.localScale = new Vector3(
            largura, Mathf.Max(altura, 0.001f), 1f
        );

        return sr;
    }

    // ENERGIA

    private void AtualizarEnergia()
    {
        if (energia >= 1f)
            return;

        energia += Time.deltaTime / Mathf.Max(tempoCargaCompleta, 0.1f);
        energia = Mathf.Clamp01(energia);
    }

    private void ArmarChuteEspecial()
    {
        if (energia < 1f || energiaArmada)
            return;

        energiaArmada = true;
    }

    private void AtualizarVisualEnergia()
    {
        if (preenchimentoBarra == null)
            return;

        float altura = 1f;

        if (GetComponent<SpriteRenderer>() is SpriteRenderer jogador &&
            jogador.sprite != null)
        {
            altura = jogador.sprite.bounds.size.y *
                alturaRelativa;
        }
        else
        {
            altura *= 2f;
        }

        float preenchido = altura * energia;

        preenchimentoBarra.enabled = energia > 0f;

        preenchimentoBarra.transform.localScale = new Vector3(
            preenchimentoBarra.transform.localScale.x,
            Mathf.Max(preenchido, 0.001f),
            1f
        );

        preenchimentoBarra.transform.localPosition = new Vector3(
            0f,
            -altura / 2f + preenchido / 2f,
            -0.01f
        );

        bool completa = energia >= 1f;

        float pulso = (Mathf.Sin(Time.time * 10f) + 1f) * 0.5f;

        float intensidade = completa
            ? (energiaArmada ? 0.65f + pulso * 0.35f : 0.45f + pulso * 0.25f)
            : 1f;

        preenchimentoBarra.color = new Color(
            corCarregada.r,
            corCarregada.g,
            corCarregada.b,
            completa ? intensidade : 0.9f
        );

        brilhoBarra.enabled = completa;

        if (completa)
        {
            brilhoBarra.color = new Color(
                corCarregada.r,
                corCarregada.g,
                corCarregada.b,
                energiaArmada ? 0.5f + pulso * 0.4f : 0.2f + pulso * 0.2f
            );
        }
    }

    // CONFIGURACAO

    public void ConfigurarControle(
        bool ia, PongBall b,
        KeyCode cima, KeyCode baixo)
    {
        KeyCode especial = cima == KeyCode.W
            ? KeyCode.LeftShift
            : KeyCode.RightShift;

        ConfigurarControle(ia, b, cima, baixo, especial);
    }

    public void ConfigurarControle(
        bool ia, PongBall b,
        KeyCode cima, KeyCode baixo,
        KeyCode especial)
    {
        controladoPorIA = ia;
        bola = b;
        teclaCima = cima;
        teclaBaixo = baixo;
        teclaEnergia = especial;
    }

    public void DefinirPartidaAtiva(bool ativa)
    {
        partidaAtiva = ativa;
        direcao = 0f;
    }

    public bool TentarConsumirEnergia(out float multiplicador)
    {
        multiplicador = 1f;

        if (!energiaArmada || energia < 1f)
            return false;

        energia = 0f;
        energiaArmada = false;

        multiplicador = multiplicadorChute;

        AtualizarVisualEnergia();

        return true;
    }

    public void ReiniciarEnergia()
    {
        energia = 0f;
        energiaArmada = false;

        AtualizarVisualEnergia();
    }
}
