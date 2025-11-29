# 🃏 Visualizador de Truco em Tempo Real - Implementação Completa

## ✅ O que foi implementado

### Backend (C#)

1. **Novo Endpoint REST API**
   - `GET /api/matches/{matchId}/state` - Retorna o estado completo da partida
   - Retorna: placar, jogadores, cartas, manilha, turno atual, etc.

2. **DTOs Criados**
   - `MatchStateDto` - Estado completo da partida
   - `PlayerStateDto` - Estado de cada jogador
   - `CardDto` - Dados de cada carta
   - `PlayedCardDto` - Cartas jogadas na mesa

3. **Método no Mediator**
   - `GetMatchState(string matchId)` - Busca o estado atual da partida
   - Mapeia os dados do `TrucoService` para os DTOs

4. **Suporte a Arquivos Estáticos**
   - Configurado para servir arquivos da pasta `Site/`
   - Acessível via navegador

5. **CORS Habilitado**
   - Permite acesso do frontend ao backend

### Frontend (Vue.js)

1. **Estrutura Criada**
   - `Site/index.html` - Página principal
   - `Site/app.js` - Lógica Vue.js com componentes
   - `Site/styles.css` - Estilos completos com animações

2. **Componentes Vue.js**
   - `TrucoViewer` - Componente principal
   - `PlayerHand` - Visualização das mãos dos jogadores
   - `CardComponent` - Renderização de cartas individuais

3. **Funcionalidades**
   - ✅ Input para Match ID
   - ✅ Polling automático a cada 1 segundo
   - ✅ Botões Carregar/Parar
   - ✅ Placar em tempo real (partida + rodada)
   - ✅ Layout em cruz (4 posições)
   - ✅ Manilha no centro
   - ✅ Cartas jogadas em posições corretas
   - ✅ Indicador do jogador atual
   - ✅ Cartas ocultas (card_back.png quando Hide=true)
   - ✅ Detecção de limpeza da mesa
   - ✅ Mensagens de erro

4. **Estilização**
   - Design responsivo
   - Mesa de jogo com fundo verde
   - Animações suaves
   - Indicador visual do jogador atual (borda dourada)
   - Efeitos hover nas cartas
   - Gradiente de fundo

### Documentação

1. **Site/README.md** - Documentação completa do visualizador
2. **Site/USAGE_EXAMPLE.md** - Exemplos práticos de uso

## 📐 Layout Implementado

```
        [Player 0 - Topo]
       Team A, Position 0
              |
              v
              
[Player 3]  MANILHA  [Player 1]
Team B,    + CARTAS   Team B,
Position 3  JOGADAS   Position 0
 (Esquerda)           (Direita)
              
              ^
              |
        [Player 2 - Baixo]
       Team A, Position 1
```

## 🎮 Como Funciona

### 1. Carregamento Inicial
```javascript
// Usuário digita Match ID e clica em "Carregar"
loadMatch() → GET /api/matches/{matchId}/state
```

### 2. Polling em Tempo Real
```javascript
// A cada 1 segundo
setInterval(() => {
    refreshMatchState() → GET /api/matches/{matchId}/state
}, 1000)
```

### 3. Detecção de Mesa Limpa
```javascript
// Se tinha cartas e agora está vazio = CloseMatch foi chamado
if (oldState.playedCards.length > 0 && newState.playedCards.length === 0) {
    console.log('Mesa limpa!');
}
```

### 4. Renderização de Cartas
```javascript
// Se Hide = true
card_back.png

// Se Hide = false
card_{suit}_{rank}.png
// Exemplo: card_hearts_A.png, card_spades_07.png
```

## 📊 Dados da API

### Endpoint
```
GET /api/matches/{matchId}/state
```

### Resposta
```json
{
  "success": true,
  "data": {
    "matchId": "game-001",
    "matchScoreTeamA": 3,      // Pontos totais Time A
    "matchScoreTeamB": 0,      // Pontos totais Time B
    "turnScoreTeamA": 1,       // Pontos da rodada Time A
    "turnScoreTeamB": 0,       // Pontos da rodada Time B
    "currentReward": 1,        // Valor em jogo
    "state": "WAITING_MOVE",   // Estado da partida
    "manilha": { ... },        // Carta manilha
    "players": [ ... ],        // 4 jogadores
    "playedCards": [ ... ],    // Cartas na mesa
    "currentTurn": 2,          // Turno atual (0-3)
    "currentPlayerId": 3       // ID do jogador atual
  }
}
```

## 🎨 Elementos Visuais

### Placar
- **Grande**: Pontos totais da partida
- **Pequeno**: (Pontos da rodada)
- **Centro**: Recompensa atual + Estado

### Mesa
- **Fundo verde**: Simula mesa de truco
- **Forma circular**: Layout redondo
- **4 posições fixas**: Top, Right, Bottom, Left

### Cartas
- **Pequenas**: 60px (nas mãos dos jogadores)
- **Médias**: 90px (manilha e cartas jogadas)
- **Grandes**: 120px (futuro uso)

### Indicadores
- **Borda dourada** + **pulsante**: Jogador atual
- **Emoji 👈**: Ao lado do nome
- **Animação de entrada**: Cards deslizam ao aparecer

## 🔧 Arquivos Modificados/Criados

### Backend
- ✏️ `Api/MatchesController.cs` - Adicionado endpoint GetMatchState
- ✏️ `Api/Models.cs` - Adicionados DTOs
- ✏️ `Mediator.cs` - Adicionado método GetMatchState
- ✏️ `Program.cs` - Configurado para servir arquivos estáticos

### Frontend (Novos)
- ➕ `Site/index.html`
- ➕ `Site/app.js`
- ➕ `Site/styles.css`
- ➕ `Site/README.md`
- ➕ `Site/USAGE_EXAMPLE.md`

## 🚀 Como Usar

### 1. Inicie o servidor
```powershell
cd g:\projetos\truco-net
dotnet run
```

### 2. Crie uma partida
```powershell
# Via Swagger: http://localhost:5002
# POST /api/commands/start-match
```

### 3. Abra o visualizador
```
http://localhost:5002/index.html
```

### 4. Digite o Match ID e clique em "Carregar Partida"

## ✨ Recursos Especiais

### Cartas Ocultas
Quando um jogador define `Hide = true` em uma carta, o visualizador mostra automaticamente `card_back.png` em vez da carta real.

### Limpeza Automática
Quando `CloseMatch` é chamado, o array `playedCards` é esvaziado no backend, e o frontend detecta automaticamente e limpa a mesa visualmente.

### Atualização em Tempo Real
O polling a cada 1 segundo garante que a interface está sempre sincronizada com o estado real da partida no backend.

### Indicador Visual
O jogador atual é destacado com:
- Borda dourada brilhante
- Emoji indicador
- Animação pulsante

## 🎯 Próximas Melhorias Possíveis

- [ ] WebSocket para atualização instantânea (em vez de polling)
- [ ] Sons ao jogar cartas
- [ ] Histórico de jogadas
- [ ] Chat entre jogadores
- [ ] Modo replay
- [ ] Estatísticas da partida
- [ ] Tema dark/light
- [ ] Modo mobile otimizado
- [ ] Indicador de conexão
- [ ] Toast notifications

## 📝 Notas Técnicas

### Por que Polling?
- Simples de implementar
- Não requer WebSocket
- Funciona com infraestrutura REST existente
- 1 segundo de intervalo é suficiente para tempo real

### Por que Vue.js via CDN?
- Não requer build step
- Deploy simples (apenas arquivos estáticos)
- Carregamento rápido
- Fácil manutenção

### Estrutura de Posições
- Posição 0 (Topo): Team A, Player 0
- Posição 1 (Direita): Team B, Player 0
- Posição 2 (Baixo): Team A, Player 1
- Posição 3 (Esquerda): Team B, Player 1

Esta estrutura mapeia naturalmente para a disposição visual em cruz.

## 🎉 Conclusão

O visualizador está 100% funcional e pronto para uso! Ele mostra em tempo real:
- ✅ Estado completo da partida
- ✅ Placar atualizado
- ✅ Cartas de todos os jogadores
- ✅ Manilha no centro
- ✅ Cartas jogadas na mesa
- ✅ Indicador do jogador atual
- ✅ Suporte a cartas ocultas
- ✅ Limpeza automática da mesa

Divirta-se jogando Truco! 🃏🎮
