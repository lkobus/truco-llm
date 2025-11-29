# 🃏 Truco Game Viewer

Visualizador em tempo real para partidas de Truco usando Vue.js.

## 📋 Características

- **Visualização em tempo real** da partida
- **Layout em cruz** com 4 jogadores (topo, direita, baixo, esquerda)
- **Manilha no centro** da mesa
- **Cartas ocultas** quando `Hide = true` (mostra o verso da carta)
- **Placar atualizado** mostrando pontuação de partida e rodada
- **Indicador visual** do jogador atual
- **Atualização automática** a cada 1 segundo
- **Limpeza automática** da mesa quando `CloseMatch` é chamado

## 🚀 Como Usar

### 1. Inicie o servidor backend

```powershell
cd g:\projetos\truco-net
dotnet run
```

O servidor estará disponível em `http://localhost:5002`

### 2. Acesse o visualizador

Abra seu navegador e acesse:
```
http://localhost:5002/index.html
```

### 3. Visualize uma partida

1. Digite o **Match ID** da partida que deseja visualizar
2. Clique em **"Carregar Partida"**
3. A partida será atualizada automaticamente em tempo real
4. Para parar a atualização automática, clique em **"Parar"**

## 📐 Layout da Mesa

```
        [Player 0 - Topo]
              ↓
              
[Player 3]  MANILHA  [Player 1]
 (Esquerda)  + MESA   (Direita)
              
              ↑
        [Player 2 - Baixo]
```

### Posições dos Jogadores

- **Posição 0**: Topo (Team A, Player 0)
- **Posição 1**: Direita (Team B, Player 0)
- **Posição 2**: Baixo (Team A, Player 1)
- **Posição 3**: Esquerda (Team B, Player 1)

## 🎮 API Endpoint

O visualizador usa o seguinte endpoint:

```
GET /api/matches/{matchId}/state
```

### Resposta JSON

```json
{
  "success": true,
  "message": "Estado da partida recuperado com sucesso",
  "data": {
    "matchId": "match1",
    "matchScoreTeamA": 6,
    "matchScoreTeamB": 3,
    "turnScoreTeamA": 1,
    "turnScoreTeamB": 0,
    "currentReward": 1,
    "state": "WAITING_MOVE",
    "manilha": {
      "name": "4 of Hearts",
      "suit": "Hearts",
      "rank": 4,
      "suitRank": 1,
      "hide": false
    },
    "players": [
      {
        "id": 1,
        "name": "Jogador 1",
        "position": 0,
        "hand": [
          {
            "name": "5 of Spades",
            "suit": "Spades",
            "rank": 5,
            "suitRank": 4,
            "hide": false
          }
        ]
      }
    ],
    "playedCards": [
      {
        "playerId": 1,
        "playerName": "Jogador 1",
        "card": {
          "name": "7 of Diamonds",
          "suit": "Diamonds",
          "rank": 7,
          "suitRank": 2,
          "hide": false
        }
      }
    ],
    "currentTurn": 1,
    "currentPlayerId": 2
  }
}
```

## 🎨 Recursos Visuais

### Placar

- **Pontuação grande**: Pontos totais da partida
- **Pontuação pequena (entre parênteses)**: Pontos da rodada atual
- **Recompensa atual**: Valor em jogo na rodada
- **Badge de estado**: Estado atual da partida (FIRST_MOVE, WAITING_MOVE, TRUCO, etc)

### Cartas

- **Cartas visíveis**: Mostram a carta real dos assets
- **Cartas ocultas** (`hide: true`): Mostram `card_back.png`
- **Manilha**: Sempre visível no centro da mesa
- **Cartas jogadas**: Aparecem em cruz ao redor da manilha

### Indicadores

- **Borda dourada**: Indica o jogador da vez
- **Emoji 👈**: Aparece ao lado do nome do jogador atual
- **Animação pulsante**: No indicador do jogador atual

## 🔄 Ciclo de Atualização

1. **Polling a cada 1 segundo** busca o estado atualizado da partida
2. **Detecta mudanças** no estado do jogo
3. **Atualiza a interface** automaticamente
4. **Limpa a mesa** quando detecta que `playedCards` está vazio (após `CloseMatch`)

## 📁 Estrutura de Arquivos

```
Site/
├── index.html    # Página principal
├── app.js        # Lógica Vue.js e componentes
└── styles.css    # Estilos CSS

assets/
├── card_back.png         # Verso da carta (para cartas ocultas)
├── ace_of_hearts.png    # Ás de copas
├── 2_of_hearts.png      # 2 de copas
└── ...                   # Outras cartas
```

## 🎯 Mapeamento de Cartas

### Naipes (Suits)
- `Hearts` → `hearts`
- `Diamonds` → `diamonds`
- `Clubs` → `clubs`
- `Spades` → `spades`

### Ranks
- `1` → `ace`
- `2-7` → `2-7`
- `10` → `jack`
- `11` → `queen`
- `12` → `king`

### Nome do Arquivo
Formato: `{rank}_of_{suit}.png`

Exemplos:
- Ás de Copas: `ace_of_hearts.png`
- 7 de Espadas: `7_of_spades.png`
- Dama de Ouros: `queen_of_diamonds.png`

## 🐛 Troubleshooting

### Partida não carrega
- Verifique se o Match ID está correto
- Confirme que a partida foi criada no backend
- Verifique se o servidor está rodando na porta 5002

### Imagens não aparecem
- Verifique se os arquivos PNG estão na pasta `assets/`
- Confira se os nomes dos arquivos seguem o padrão correto
- Abra o console do navegador (F12) para ver erros

### Não atualiza em tempo real
- Verifique a conexão com o backend
- Confirme que o polling está ativo (botão "Parar" visível)
- Verifique o console do navegador para erros de CORS

## 💡 Dicas

- Use o **Match ID** que você criou ao iniciar uma partida
- A atualização é automática, não precisa recarregar a página
- O indicador **👈** mostra de quem é a vez
- Cartas com `hide: true` aparecem como verso da carta
- Quando a mesa é limpa (`CloseMatch`), as cartas desaparecem automaticamente

## 🔧 Tecnologias Utilizadas

- **Vue.js 3** (via CDN) - Framework JavaScript
- **Axios** (via CDN) - Cliente HTTP
- **CSS3** - Estilização com animações
- **ASP.NET Core** - Backend API
