# Script para testar o estado da partida
# Use: .\test-match-state.ps1 -MatchId "seu-match-id"

param(
    [string]$MatchId = "game-001"
)

Write-Host "Verificando estado da partida: $MatchId" -ForegroundColor Cyan
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5002/api/matches/$MatchId/state" -Method Get
    
    if ($response.success) {
        $state = $response.data
        
        Write-Host "=== ESTADO DA PARTIDA ===" -ForegroundColor Green
        Write-Host "Match ID: $($state.matchId)"
        Write-Host "Placar Time A: $($state.matchScoreTeamA) (Rodada: $($state.turnScoreTeamA))"
        Write-Host "Placar Time B: $($state.matchScoreTeamB) (Rodada: $($state.turnScoreTeamB))"
        Write-Host "Estado: $($state.state)"
        Write-Host "Recompensa: $($state.currentReward)"
        Write-Host "Turno: $($state.currentTurn)"
        Write-Host ""
        
        Write-Host "=== MANILHA ===" -ForegroundColor Yellow
        if ($state.manilha) {
            Write-Host "Carta: $($state.manilha.name) de $($state.manilha.suit) (Rank: $($state.manilha.rank))"
        } else {
            Write-Host "Nenhuma manilha"
        }
        Write-Host ""
        
        Write-Host "=== JOGADORES ===" -ForegroundColor Cyan
        foreach ($player in $state.players) {
            Write-Host "Player $($player.id): $($player.name) (Posição: $($player.position))"
            Write-Host "  Cartas na mão: $($player.hand.Count)"
            foreach ($card in $player.hand) {
                Write-Host "    - $($card.name) de $($card.suit) (Rank: $($card.rank), Hide: $($card.hide))"
            }
        }
        Write-Host ""
        
        Write-Host "=== CARTAS JOGADAS ===" -ForegroundColor Magenta
        Write-Host "Total na mesa: $($state.playedCards.Count)"
        foreach ($played in $state.playedCards) {
            Write-Host "  Player $($played.playerId) ($($played.playerName)): $($played.card.name) de $($played.card.suit)"
        }
        
    } else {
        Write-Host "Erro: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "Erro ao conectar: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Certifique-se de que o servidor está rodando em http://localhost:5002" -ForegroundColor Yellow
}
