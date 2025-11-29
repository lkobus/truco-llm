# 🎮 Exemplo de Uso do Visualizador

Este arquivo contém exemplos de requisições para testar o visualizador em tempo real.

## Passo 1: Inicie o servidor

```powershell
dotnet run
```

O servidor estará disponível em `http://localhost:5002`

## Passo 2: Crie uma partida

### Via Swagger UI
Acesse `http://localhost:5002` e use o endpoint:

```
POST /api/commands/start-match
```

### Via cURL

```bash
curl -X POST http://localhost:5002/api/commands/start-match \
  -H "Content-Type: application/json" \
  -d '{
    "matchId": "game-001",
    "teamA": [
      {
        "id": 1,
        "name": "Alice",
        "type": "RandomCardPlayer"
      },
      {
        "id": 2,
        "name": "Bob",
        "type": "RandomCardPlayer"
      }
    ],
    "teamB": [
      {
        "id": 3,
        "name": "Carlos",
        "type": "RandomCardPlayer"
      },
      {
        "id": 4,
        "name": "Diana",
        "type": "RandomCardPlayer"
      }
    ],
    "startRoundPlayer": 1
  }'
```

### Via PowerShell

```powershell
$body = @{
    matchId = "game-001"
    teamA = @(
        @{
            id = 1
            name = "Alice"
            type = "RandomCardPlayer"
        },
        @{
            id = 2
            name = "Bob"
            type = "RandomCardPlayer"
        }
    )
    teamB = @(
        @{
            id = 3
            name = "Carlos"
            type = "RandomCardPlayer"
        },
        @{
            id = 4
            name = "Diana"
            type = "RandomCardPlayer"
        }
    )
    startRoundPlayer = 1
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "http://localhost:5002/api/commands/start-match" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

## Passo 3: Abra o Visualizador

Abra seu navegador e acesse:
```
http://localhost:5002/index.html
```

Digite `game-001` no campo Match ID e clique em "Carregar Partida"

## Passo 4: Jogue!

Os jogadores automáticos (RandomCardPlayer) jogarão sozinhos. Você verá:
- ✅ Cartas sendo jogadas na mesa
- ✅ Placar sendo atualizado
- ✅ Indicador do jogador atual
- ✅ Mesa sendo limpa após cada rodada

## 🧪 Testando com LLMPlayer

Se quiser usar jogadores com IA (LLMPlayer), você precisa fornecer uma API Key:

```json
{
  "matchId": "game-ai",
  "teamA": [
    {
      "id": 1,
      "name": "AI Player 1",
      "type": "LLMPlayer",
      "apiKey": "sua-api-key-aqui"
    },
    {
      "id": 2,
      "name": "Random Player 1",
      "type": "RandomCardPlayer"
    }
  ],
  "teamB": [
    {
      "id": 3,
      "name": "AI Player 2",
      "type": "LLMPlayer",
      "apiKey": "sua-api-key-aqui"
    },
    {
      "id": 4,
      "name": "Random Player 2",
      "type": "RandomCardPlayer"
    }
  ],
  "startRoundPlayer": 1
}
```

## 📊 Verificar Estado da Partida (Manual)

Se quiser ver o estado JSON diretamente:

```
GET http://localhost:5002/api/matches/game-001/state
```

Via PowerShell:
```powershell
Invoke-RestMethod -Uri "http://localhost:5002/api/matches/game-001/state"
```

## 🎯 Endpoints Úteis

### Listar todas as partidas ativas
```
GET http://localhost:5002/api/matches
```

### Encerrar uma partida
```
DELETE http://localhost:5002/api/matches/game-001
```

### Health Check
```
GET http://localhost:5002/health
```

## 🐛 Troubleshooting

### Erro: "Partida não encontrada"
- Verifique se você criou a partida usando o endpoint `/api/commands/start-match`
- Confirme que o Match ID está correto

### Cartas não aparecem
- Certifique-se de que os arquivos PNG estão na pasta `assets/`
- Verifique se os nomes dos arquivos seguem o padrão: `{rank}_of_{suit}.png`
- Exemplo: `ace_of_hearts.png`, `7_of_spades.png`

### CORS Error
- O CORS já está habilitado no backend
- Verifique se está acessando via `http://localhost:5002` e não `file://`

### Partida não atualiza
- Verifique se o botão "Parar" está visível (indica que o polling está ativo)
- Abra o console do navegador (F12) para ver logs
- Confirme que o backend está rodando

## 💡 Dicas

1. **Múltiplas Partidas**: Você pode ter várias partidas simultâneas com IDs diferentes
2. **Atualização Automática**: O visualizador atualiza a cada 1 segundo automaticamente
3. **Performance**: Se tiver muitas partidas, considere aumentar o intervalo de polling
4. **Debug**: Use o console do navegador (F12) para ver requisições e respostas

## 📝 Estrutura de uma Partida

```
Posição dos Jogadores:
     [Alice - P1]         (Topo)
           |
[Diana - P4] --- MANILHA --- [Carlos - P3]
  (Esquerda)                   (Direita)
           |
      [Bob - P2]          (Baixo)

Time A: Alice (P1) + Bob (P2)
Time B: Carlos (P3) + Diana (P4)
```

## 🎲 Como Funciona o Jogo

1. Cada jogador recebe 3 cartas
2. Uma manilha é revelada no centro
3. Jogadores jogam cartas em turnos
4. Após 4 jogadas, a rodada é fechada (`CloseMatch`)
5. A mesa é limpa e uma nova rodada começa
6. Primeiro time a chegar em 12 pontos vence

## 🚀 Próximos Passos

- [ ] Adicionar som ao jogar cartas
- [ ] Animações mais suaves
- [ ] Chat entre jogadores
- [ ] Histórico de jogadas
- [ ] Modo replay
- [ ] Estatísticas da partida
