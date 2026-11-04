
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class PongBall : MonoBehaviour
{
    [SerializeField] private PongGameManager gerenciador;

    [Header("Velocidade")]
    [SerializeField] private float velocidadeInicial = 7f;
    [SerializeField] private float velocidadeMaxima = 13f;
    [SerializeField] private float aumentoVelocidade = 0.25f;
    [SerializeField] private float limiteVelocidadeChuteEspecial = 18f;
    [SerializeField] private float limiteHorizontal = 10f;

    [Header("Efeito Especial")]
    [SerializeField] private float duracaoEfeito = 1.2f;
    [SerializeField] private float larguraRastro = 0.35f;
    [SerializeField] private float duracaoRastro = 0.35f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer rastro;
    private Material materialRastro;

    private Color corOriginal = Color.white;
    private float velocidadeAtual;
    private float tempoEfeito;

    private bool emJogo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            corOriginal = spriteRenderer.color;

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        CriarRastro();
    }

    private void Start()
    {
        if (gerenciador == null)
            gerenciador = FindFirstObjectByType<PongGameManager>();
    }

    private void Update()
    {
        if (emJogo)
        {
            if (transform.position.x > limiteHorizontal)
                MarcarPonto(1);
            else if (transform.position.x < -limiteHorizontal)
                MarcarPonto(2);
        }

        AtualizarEfeitoEspecial();
    }

    // CRIAR EFEITO VISUAL

    private void CriarRastro()
    {
        rastro = gameObject.AddComponent<TrailRenderer>();

        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
        {
            Debug.LogWarning("PongBall: Shader para o rastro nao encontrado.");
            rastro.enabled = false;
            return;
        }

        materialRastro = new Material(shader);
        rastro.material = materialRastro;

        rastro.time = duracaoRastro;
        rastro.startWidth = larguraRastro;
        rastro.endWidth = 0f;

        rastro.widthMultiplier = 1f;
        rastro.numCapVertices = 5;
        rastro.alignment = LineAlignment.View;
        rastro.textureMode = LineTextureMode.Stretch;

        rastro.emitting = false;
        rastro.enabled = false;

        if (spriteRenderer != null)
        {
            rastro.sortingLayerID = spriteRenderer.sortingLayerID;
            rastro.sortingOrder = spriteRenderer.sortingOrder - 1;
        }
    }

    // ATIVAR CHUTE ESPECIAL

    private void AtivarEfeitoEspecial(Color cor)
    {
        tempoEfeito = duracaoEfeito;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(
                corOriginal,
                cor,
                0.85f
            );
        }

        if (rastro == null || materialRastro == null)
            return;

        Gradient gradiente = new Gradient();

        gradiente.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(cor, 0.35f),
                new GradientColorKey(cor, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.4f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        rastro.colorGradient = gradiente;

        rastro.startWidth = larguraRastro * 1.5f;
        rastro.time = duracaoRastro;

        rastro.Clear();
        rastro.enabled = true;
        rastro.emitting = true;
    }

    private void AtualizarEfeitoEspecial()
    {
        if (tempoEfeito <= 0f)
            return;

        tempoEfeito -= Time.deltaTime;

        if (spriteRenderer != null)
        {
            float pulso =
                (Mathf.Sin(Time.time * 25f) + 1f) * 0.5f;

            spriteRenderer.color = Color.Lerp(
                corOriginal,
                Color.white,
                0.35f + pulso * 0.35f
            );
        }

        if (tempoEfeito <= 0f)
            EncerrarEfeitoEspecial();
    }

    private void EncerrarEfeitoEspecial()
    {
        tempoEfeito = 0f;

        if (spriteRenderer != null)
            spriteRenderer.color = corOriginal;

        if (rastro != null)
        {
            rastro.emitting = false;
            rastro.time = duracaoRastro;
        }
    }

    // PREPARAR NOVA RODADA

    public void PrepararNovaRodada()
    {
        emJogo = false;
        velocidadeAtual = velocidadeInicial;

        rb.linearVelocity = Vector2.zero;
        rb.position = Vector2.zero;

        EncerrarEfeitoEspecial();

        if (rastro != null)
        {
            rastro.Clear();
            rastro.enabled = false;
        }
    }

    // LANCAMENTO

    public void Lancar()
    {
        if (emJogo)
            return;

        Vector2 direcao = new Vector2(
            Random.value < 0.5f ? -1f : 1f,
            Random.Range(-0.55f, 0.55f)
        ).normalized;

        emJogo = true;

        rb.linearVelocity = direcao * velocidadeAtual;
    }

    public void Parar()
    {
        emJogo = false;
        rb.linearVelocity = Vector2.zero;

        EncerrarEfeitoEspecial();

        if (rastro != null)
        {
            rastro.Clear();
            rastro.enabled = false;
        }
    }

    // COLISAO COM JOGADORES

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!emJogo)
            return;

        PongPaddle paddle = c.gameObject.GetComponent<PongPaddle>();

        if (paddle == null && c.otherCollider != null)
            paddle = c.otherCollider.GetComponent<PongPaddle>();

        if (paddle == null)
            return;

        float altura = c.collider.bounds.extents.y;

        float impacto = Mathf.Clamp(
            (transform.position.y - paddle.transform.position.y) /
            Mathf.Max(altura, 0.01f),
            -1f,
            1f
        );

        float x = transform.position.x < paddle.transform.position.x
            ? -1f
            : 1f;

        Vector2 direcao = new Vector2(x, impacto).normalized;

        velocidadeAtual = Mathf.Min(
            velocidadeAtual + aumentoVelocidade,
            velocidadeMaxima
        );

        if (paddle.TentarConsumirEnergia(out float multiplicador))
        {
            velocidadeAtual = Mathf.Min(
                velocidadeAtual * multiplicador,
                limiteVelocidadeChuteEspecial
            );

            AtivarEfeitoEspecial(paddle.CorEnergia);
        }

        rb.linearVelocity = direcao * velocidadeAtual;
    }

    // PONTUACAO

    private void MarcarPonto(int jogador)
    {
        if (!emJogo)
            return;

        emJogo = false;
        rb.linearVelocity = Vector2.zero;

        EncerrarEfeitoEspecial();

        if (rastro != null)
        {
            rastro.Clear();
            rastro.enabled = false;
        }

        if (gerenciador != null)
            gerenciador.RegistrarPonto(jogador);
        else
            Debug.LogError("PongBall: PongGameManager nao encontrado.");
    }

    private void OnDestroy()
    {
        if (materialRastro != null)
            Destroy(materialRastro);
    }
}
