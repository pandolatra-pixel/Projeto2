using UnityEngine;
public class PongGameManager : MonoBehaviour
{
    public enum JogadorSelecionado{Player1,Player2}
    [Header("Referencias")]
    [SerializeField] private PongBall bola;
    [SerializeField] private PongPaddle player1,player2;
    [Header("Selecao")]
    [SerializeField] private JogadorSelecionado jogadorHumano=JogadorSelecionado.Player1;
    [SerializeField] private bool doisJogadores;
    [Header("Regras")]
    [SerializeField,Min(1)] private int pontosParaVencer=5;
    [SerializeField,Min(0f)] private float duracaoContagem=3f,atrasoEntreRodadas=1f;
    private int pontosPlayer1,pontosPlayer2;
    private bool jogoEncerrado,contando,mostrarVai;
    private float temporizadorContagem;

    private void Start()
    {
        if(bola==null)bola=FindFirstObjectByType<PongBall>();
        if(player1==null){var o=GameObject.Find("Player1");if(o)player1=o.GetComponent<PongPaddle>();}
        if(player2==null){var o=GameObject.Find("Player2");if(o)player2=o.GetComponent<PongPaddle>();}
        if(!bola||!player1||!player2){Debug.LogError("GameManager: associe Ball, Player1 e Player2.");enabled=false;return;}
        ConfigurarJogadores(); bola.PrepararNovaRodada(); IniciarContagem();
    }
    private void ConfigurarJogadores()
    {
        if(doisJogadores)
        {
            player1.ConfigurarControle(false,bola,KeyCode.W,KeyCode.S,KeyCode.LeftShift);
            player2.ConfigurarControle(false,bola,KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.RightShift);
        }
        else if(jogadorHumano==JogadorSelecionado.Player1)
        {
            player1.ConfigurarControle(false,bola,KeyCode.W,KeyCode.S,KeyCode.LeftShift);
            player2.ConfigurarControle(true,bola,KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.RightShift);
        }
        else
        {
            player1.ConfigurarControle(true,bola,KeyCode.W,KeyCode.S,KeyCode.LeftShift);
            player2.ConfigurarControle(false,bola,KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.RightShift);
        }
    }
    private void IniciarContagem()
    {
        bola.Parar();bola.PrepararNovaRodada();contando=true;mostrarVai=false;
        temporizadorContagem=duracaoContagem+atrasoEntreRodadas;
        player1.DefinirPartidaAtiva(false);player2.DefinirPartidaAtiva(false);
    }
    private void Update()
    {
        if(jogoEncerrado)
        {if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))ReiniciarJogo();return;}
        if(Input.GetKeyDown(KeyCode.Escape))Time.timeScale=Time.timeScale==0f?1f:0f;
        if(!contando||Time.timeScale==0f)return;
        temporizadorContagem-=Time.deltaTime;
        if(temporizadorContagem<=0f)
        {
            contando=false;mostrarVai=false;player1.DefinirPartidaAtiva(true);
            player2.DefinirPartidaAtiva(true);bola.Lancar();
        }
        else mostrarVai=temporizadorContagem<=.65f;
    }
    public void RegistrarPonto(int jogador)
    {
        if(jogoEncerrado||contando)return;
        if(jogador==1)pontosPlayer1++;else if(jogador==2)pontosPlayer2++;else return;
        if(pontosPlayer1>=pontosParaVencer||pontosPlayer2>=pontosParaVencer)
        {
            jogoEncerrado=true;bola.Parar();player1.DefinirPartidaAtiva(false);player2.DefinirPartidaAtiva(false);return;
        }
        IniciarContagem();
    }
    private void ReiniciarJogo()
    {
        pontosPlayer1=0;pontosPlayer2=0;jogoEncerrado=false;Time.timeScale=1f;
        player1.ReiniciarEnergia();player2.ReiniciarEnergia();IniciarContagem();
    }
    private void OnGUI()
    {
        GUIStyle score=new GUIStyle(GUI.skin.label){fontSize=36,alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};
        score.normal.textColor=Color.white;
        GUI.Label(new Rect(Screen.width*.25f-50,15,100,55),pontosPlayer1.ToString(),score);
        GUI.Label(new Rect(Screen.width*.75f-50,15,100,55),pontosPlayer2.ToString(),score);
        GUIStyle small=new GUIStyle(GUI.skin.label){fontSize=14,alignment=TextAnchor.MiddleCenter};
        small.normal.textColor=Color.white;
        GUI.Label(new Rect(0,Screen.height-35,Screen.width,25),"P1: W/S | Especial: Left Shift     P2: Setas | Especial: Right Shift     ESC: Pausar",small);
        if(contando&&!jogoEncerrado)
        {
            string t=mostrarVai?"VAI!":Mathf.Max(1,Mathf.CeilToInt(temporizadorContagem)).ToString();
            GUIStyle count=new GUIStyle(GUI.skin.label){fontSize=76,alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};
            count.normal.textColor=Color.yellow;
            GUI.Label(new Rect(0,Screen.height*.35f,Screen.width,100),t,count);
        }
        if(jogoEncerrado)
        {
            string winner=pontosPlayer1>pontosPlayer2?"PLAYER 1 VENCEU!":"PLAYER 2 VENCEU!";
            GUIStyle end=new GUIStyle(GUI.skin.label){fontSize=34,alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};
            end.normal.textColor=Color.yellow;
            GUI.Label(new Rect(0,Screen.height*.38f,Screen.width,55),winner,end);
            GUI.Label(new Rect(0,Screen.height*.48f,Screen.width,35),"Pressione ENTER para jogar novamente",small);
        }
        else if(Time.timeScale==0f)
        {
            GUIStyle pause=new GUIStyle(GUI.skin.label){fontSize=32,alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};
            pause.normal.textColor=Color.yellow;
            GUI.Label(new Rect(0,Screen.height*.42f,Screen.width,50),"JOGO PAUSADO",pause);
        }
    }
    private void OnDisable(){Time.timeScale=1f;}
}