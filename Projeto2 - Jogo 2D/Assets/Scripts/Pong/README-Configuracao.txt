PONG 2D — UNITY 6
=================

SCRIPTS
-------
PongGameManager.cs : placar, regras, pausa e reinício.
PongPaddle.cs      : controles das duas raquetes.
PongBall.cs        : movimento, colisões e pontuação.

CONFIGURAÇÃO DA CENA
--------------------

1. Crie uma cena 2D (ou use uma cena vazia com câmera ortográfica).
2. Configure a câmera:
   - Projection: Orthographic
   - Position: (0, 0, -10)
   - Size: 5
   - Background: preto ou outra cor escura.

3. Crie as raquetes:
   - GameObject > 2D Object > Sprites > Square.
   - Renomeie para Player1.
   - Position: (-8, 0, 0)
   - Scale: (0.35, 2, 1)
   - Adicione BoxCollider2D.
   - Adicione PongPaddle.
   - No PongPaddle, configure:
       Tecla Cima: W
       Tecla Baixo: S
       Velocidade: 8
       Limite Superior: 4
       Limite Inferior: -4
   - Rigidbody2D será adicionado automaticamente pelo RequireComponent.
   - Repita para Player2:
       Position: (8, 0, 0)
       Tecla Cima: UpArrow
       Tecla Baixo: DownArrow

4. Crie as paredes superior e inferior:
   - Crie um Square para cada parede.
   - Superior: Position (0, 4.7, 0), Scale (18, 0.3, 1).
   - Inferior: Position (0, -4.7, 0), Scale (18, 0.3, 1).
   - Adicione BoxCollider2D.
   - NÃO marque Is Trigger.
   - Não precisam de Rigidbody2D.

5. Crie a bola:
   - GameObject > 2D Object > Sprites > Circle.
   - Nome: Bola.
   - Position: (0, 0, 0).
   - Scale: (0.35, 0.35, 1).
   - Adicione CircleCollider2D.
   - Adicione PongBall.
   - O Rigidbody2D será adicionado automaticamente.
   - Ajuste o campo Limite Horizontal para 10.

6. Crie o gerenciador:
   - Crie um Empty GameObject chamado GameManager.
   - Adicione PongGameManager.
   - Arraste o objeto Bola para o campo Bola.
   - Pontos Para Vencer: 5.

7. Ajuste a câmera e teste com Play.

CONTROLES
---------
Player 1: W = subir | S = descer
Player 2: Seta para cima = subir | Seta para baixo = descer
ESC = pausar/retomar
ENTER = reiniciar após a vitória

IMPORTANTE: este pacote usa Input.GetKey (Input Manager clássico).
Em Edit > Project Settings > Player > Other Settings > Active Input Handling,
selecione Both ou Input Manager (Old). Reinicie o Editor se solicitado.

NOTAS
-----
- A pontuação aparece na tela usando OnGUI, sem precisar criar Canvas.
- O jogo termina quando um jogador alcança a pontuação configurada.
- Após o fim, pressione ENTER para reiniciar.
- A bola acelera gradualmente ao bater nas raquetes.
