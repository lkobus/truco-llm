# Truco.NET

Sistema de gerenciamento de partidas de truco com arquitetura baseada em filas de comandos por partida, suporte a múltiplos tipos de jogadores (IA e humanos) e visualizador web em tempo real.

## 🎮 Visualizador em Tempo Real

Visualizador web em tempo real para acompanhar as partidas de Truco com suporte a comentários/trash talking!

### Acesso Rápido
```
http://localhost:5002/index.html
```

### Recursos do Visualizador
- ✅ Interface Vue.js com atualização automática (1 segundo)
- ✅ Layout em cruz com 4 jogadores
- ✅ Placar em tempo real (partida + rodada)
- ✅ Manilha visível no centro da mesa
- ✅ Cartas jogadas posicionadas ao redor
- ✅ Indicador visual do jogador atual
- ✅ Suporte a cartas ocultas (hide = true)
- ✅ Limpeza automática da mesa após cada rodada
- ✅ Sistema de comentários/trash talking dos jogadores
- ✅ Indicador de partida finalizada com vencedor

📖 **[Ver documentação completa do visualizador →](Site/README.md)**

## Arquitetura

### Componentes Principais

- **Mediator**: Gerencia múltiplas partidas simultâneas, cada uma com seu próprio Channel (fila)
- **TrucoService**: Gerencia a lógica do jogo de truco (rodadas, pontuação, cartas)
- **GameService**: Serviço auxiliar para operações de jogo
- **ICommand**: Interface para comandos que são enfileirados e processados
- **Player Types**: Suporte a diferentes tipos de jogadores (RandomCard, LLM, Gemini)
- **CommentQueue**: Sistema de comentários/trash talking por partida
- **REST API**: Endpoints para criar partidas, enviar comandos e obter estados
- **Logging**: Serilog para logs estruturados em console e arquivo

### Fluxo de Execução

1. Cliente inicia uma partida via POST `/api/commands/start-match` com configuração de times e tipos de jogadores
2. Mediator cria um Channel exclusivo para essa partida
3. TrucoService gerencia a lógica do jogo (distribuição de cartas, manilha, pontuação)
4. Jogadores IA (LLM/Gemini) tomam decisões automaticamente usando APIs de LLM
5. Estados da partida podem ser consultados via GET `/api/matches/{matchId}/state`
6. Comentários dos jogadores são coletados via GET `/api/matches/{matchId}/comments`
7. Visualizador web atualiza em tempo real

## Como Rodar

```powershell
# Restaurar dependências
dotnet restore

# Executar
dotnet run
```

A API estará disponível em: http://localhost:5002

### Visualizador Web
Acesse o visualizador em tempo real: http://localhost:5002/index.html

## Endpoints da API

### Health Check
```http
GET /health
```

### Iniciar Partida
```http
POST /api/commands/start-match
Content-Type: application/json

{
  "matchId": "partida-123",  // Opcional - será gerado automaticamente se omitido
  "teamA": [
    {
      "id": 1,
      "name": "Jogador 1",
      "type": "RandomCardPlayer"  // ou "LLMPlayer" ou "GeminiPlayer"
    },
    {
      "id": 2,
      "name": "Jogador 2",
      "type": "GeminiPlayer",
      "apiKey": "sua-api-key-aqui"  // Obrigatório para LLMPlayer e GeminiPlayer
    }
  ],
  "teamB": [
    {
      "id": 3,
      "name": "Jogador 3",
      "type": "LLMPlayer",
      "apiKey": "sua-api-key-aqui"
    },
    {
      "id": 4,
      "name": "Jogador 4",
      "type": "RandomCardPlayer"
    }
  ],
  "startRoundPlayer": 1  // ID do jogador que inicia a rodada
}
```

### Listar Partidas Ativas
```http
GET /api/matches
```

### Obter Estado da Partida
```http
GET /api/matches/{matchId}/state
```

Retorna o estado completo da partida incluindo:
- Placar da partida (TeamA vs TeamB)
- Placar da rodada atual
- Jogadores e suas cartas (com suporte a hide)
- Manilha
- Cartas jogadas na mesa
- Jogador atual
- Turno atual
- Estado da partida (Playing, Finished, etc.)
- Comentários dos jogadores
- Indicador de partida finalizada e vencedor

### Obter Comentários da Partida
```http
GET /api/matches/{matchId}/comments
```

Retorna e remove os comentários pendentes dos jogadores (trash talking).

### Encerrar Partida
```http
DELETE /api/matches/{matchId}
```

## Exemplos com cURL

```bash
# Iniciar partida com jogadores IA
curl -X POST http://localhost:5002/api/commands/start-match \
  -H "Content-Type: application/json" \
  -d '{
    "teamA": [
      {"id": 1, "name": "Player 1", "type": "RandomCardPlayer"},
      {"id": 2, "name": "Player 2", "type": "RandomCardPlayer"}
    ],
    "teamB": [
      {"id": 3, "name": "Player 3", "type": "RandomCardPlayer"},
      {"id": 4, "name": "Player 4", "type": "RandomCardPlayer"}
    ],
    "startRoundPlayer": 1
  }'

# Obter estado da partida
curl http://localhost:5002/api/matches/{matchId}/state

# Obter comentários
curl http://localhost:5002/api/matches/{matchId}/comments

# Listar partidas
curl http://localhost:5002/api/matches

# Encerrar partida
curl -X DELETE http://localhost:5002/api/matches/{matchId}
```

## Exemplos com PowerShell

```powershell
# Iniciar partida com jogadores IA
$body = @{
    teamA = @(
        @{ id = 1; name = "Player 1"; type = "RandomCardPlayer" },
        @{ id = 2; name = "Player 2"; type = "RandomCardPlayer" }
    )
    teamB = @(
        @{ id = 3; name = "Player 3"; type = "RandomCardPlayer" },
        @{ id = 4; name = "Player 4"; type = "RandomCardPlayer" }
    )
    startRoundPlayer = 1
} | ConvertTo-Json -Depth 3

$response = Invoke-RestMethod -Uri "http://localhost:5002/api/commands/start-match" `
    -Method Post -Body $body -ContentType "application/json"
$matchId = $response.data.matchId

# Obter estado da partida
Invoke-RestMethod -Uri "http://localhost:5002/api/matches/$matchId/state"

# Obter comentários
Invoke-RestMethod -Uri "http://localhost:5002/api/matches/$matchId/comments"

# Listar partidas
Invoke-RestMethod -Uri "http://localhost:5002/api/matches"
```

## Estrutura de Diretórios

```
truco-net/
├── Api/
│   ├── CommandsController.cs    # Endpoints para comandos (start-match)
│   ├── MatchesController.cs     # Endpoints para gerenciar partidas
│   └── Models.cs                # DTOs da API (requests/responses)
├── Commands/
│   ├── ICommand.cs              # Interface base para comandos
│   ├── StartMatchCommand.cs     # Comando para iniciar partida
│   ├── CloseMatchCommand.cs     # Comando para encerrar partida
│   └── Players/                 # Comandos específicos de jogadores
│       ├── PlayerCommand.cs     # Comando base para ações de jogadores
│       └── OnTrucoCommand.cs    # Comando para responder ao truco
├── Infrastructure/
│   └── CommentQueue.cs          # Sistema de filas de comentários
├── Models/
│   └── GameMatch.cs             # Modelo de partida (obsoleto)
├── Truco/
│   ├── GameService.cs           # Serviço auxiliar do jogo
│   ├── TrucoService.cs          # Lógica principal do jogo de truco
│   └── Entities/
│       ├── Card.cs              # Modelo de carta
│       ├── Match.cs             # Modelo de partida
│       ├── Player.cs            # Classe base para jogadores
│       ├── TrucoDeck.cs         # Baralho de truco
│       ├── Actions/
│       │   └── GameAction.cs    # Ações do jogo
│       ├── Match/
│       │   └── StateEnum.cs     # Estados da partida
│       └── Players/
│           ├── RandomCardPlayer.cs  # Jogador que joga cartas aleatórias
│           ├── LLMPlayer.cs         # Jogador baseado em LLM genérico
│           └── GeminiPlayer.cs      # Jogador baseado em Gemini AI
├── Site/                        # Visualizador Web
│   ├── index.html               # Interface Vue.js
│   ├── app.js                   # Lógica do visualizador
│   ├── styles.css               # Estilos
│   ├── README.md                # Documentação do visualizador
│   ├── USAGE_EXAMPLE.md         # Exemplos de uso
│   ├── IMPLEMENTATION.md        # Detalhes de implementação
│   └── CARDS_REFERENCE.md       # Referência das cartas
├── assets/                      # Imagens das cartas
│   ├── card_back.png            # Verso da carta
│   ├── card_hearts_A.png        # Cartas de copas
│   ├── card_diamonds_07.png     # Cartas de ouros
│   └── ...                      # Outras cartas
├── Examples/
│   ├── LLMPlayerExample.cs      # Exemplo de uso de jogador LLM
│   └── StartMatchRequests.json  # Exemplos de requisições
├── Mediator.cs                  # Gerenciador de filas por partida
├── Program.cs                   # Ponto de entrada e configuração
├── appsettings.json             # Configurações
└── test-match-state.ps1         # Script de teste
```

## Características Principais

✅ **Múltiplas Partidas Simultâneas**: Cada partida tem seu próprio Channel/fila  
✅ **Tipos de Jogadores**: RandomCard, LLMPlayer, GeminiPlayer com suporte a IA  
✅ **Processamento Assíncrono**: Comandos são processados em background  
✅ **Thread-Safe**: Uso de ConcurrentDictionary e Channels  
✅ **Logging Estruturado**: Serilog com output em console e arquivo (pasta logs/)  
✅ **Injeção de Dependência**: ASP.NET Core DI  
✅ **REST API**: Endpoints documentados com Swagger (http://localhost:5002)  
✅ **Extensível**: Fácil adicionar novos comandos e tipos de jogadores  
✅ **Visualizador Web**: Interface em tempo real com Vue.js  
✅ **Suporte a Cartas Ocultas**: Sistema de hide para privacidade  
✅ **Sistema de Comentários**: CommentQueue para trash talking dos jogadores  
✅ **Lógica Completa do Truco**: TrucoService com regras, manilha, pontuação  
✅ **Integração com IA**: Suporte a LLMs para jogadores inteligentes  

## 🚀 Quick Start com Visualizador

1. **Inicie o servidor**
   ```powershell
   dotnet run
   ```

2. **Acesse o Swagger** em http://localhost:5002

3. **Crie uma partida** usando o endpoint `POST /api/commands/start-match`
   - Configure os times com diferentes tipos de jogadores
   - Copie o Match ID retornado

4. **Abra o visualizador** em http://localhost:5002/index.html

5. **Digite o Match ID e clique em "Carregar Partida"**

6. **Assista o jogo em tempo real!** 🎮
   - Veja as cartas sendo jogadas
   - Acompanhe o placar
   - Leia os comentários dos jogadores

## Tipos de Jogadores

- **RandomCardPlayer**: Joga cartas aleatórias (não requer API key)
- **LLMPlayer**: Usa LLM genérico para tomar decisões (requer API key)
- **GeminiPlayer**: Usa Google Gemini AI (requer API key do Gemini)

## Próximos Passos

- [x] Implementar visualizador em tempo real
- [x] Implementar lógica completa do jogo de truco
- [x] Suporte a múltiplos tipos de jogadores com IA
- [x] Sistema de comentários/trash talking
- [ ] Adicionar WebSockets para notificações instantâneas
- [ ] Adicionar persistência de dados
- [ ] Implementar autenticação/autorização
- [ ] Implementar sistema de ranking
- [ ] Adicionar testes unitários
- [ ] Sons e animações no visualizador
- [ ] Histórico completo de jogadas
- [ ] Modo multiplayer com jogadores humanos
