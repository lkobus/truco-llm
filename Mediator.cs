using System.Collections.Concurrent;
using System.Threading.Channels;
using Serilog;
using truco_net.Commands;
using truco_net.Truco;

namespace truco_net;

public class Mediator
{
    // Dicionário de channels por partida
    private readonly ConcurrentDictionary<string, Channel<ICommand>> _matchChannels = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _matchCancellationTokens = new();

    public readonly TrucoService TrucoService;
    public readonly TrucoNet.Infrastructure.CommentQueue CommentQueue;

    public Mediator(TrucoService trucoService, TrucoNet.Infrastructure.CommentQueue commentQueue)
    {
        TrucoService = trucoService;
        CommentQueue = commentQueue;
    }    

    /// <summary>
    /// Cria um novo channel para uma partida e inicia o processamento
    /// </summary>
    public Task<bool> CreateMatch(string matchId)
    {
        if (_matchChannels.ContainsKey(matchId))
        {
            Log.Warning("Tentativa de criar partida {MatchId} que já existe", matchId);
            return Task.FromResult(false);
        }

        var channel = Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var cts = new CancellationTokenSource();

        if (_matchChannels.TryAdd(matchId, channel) && 
            _matchCancellationTokens.TryAdd(matchId, cts))
        {
            Log.Information("Partida {MatchId} criada, iniciando processamento de comandos", matchId);
            
            // Inicia o processamento dos comandos em background
            _ = Task.Run(() => ProcessMatchCommands(matchId, channel.Reader, cts.Token));
            
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Enfileira um comando para uma partida específica
    /// </summary>
    public async Task<bool> EnqueueCommand(string matchId, ICommand command)
    {
        if (_matchChannels.TryGetValue(matchId, out var channel))
        {
            await channel.Writer.WriteAsync(command);
            Log.Debug("Comando {CommandType} enfileirado para partida {MatchId}", 
                command.GetType().Name, matchId);
            return true;
        }

        Log.Warning("Tentativa de enfileirar comando para partida inexistente {MatchId}", matchId);
        return false;
    }

    /// <summary>
    /// Processa comandos de uma partida específica
    /// </summary>
    private async Task ProcessMatchCommands(string matchId, ChannelReader<ICommand> reader, CancellationToken cancellationToken)
    {
        Log.Information("Iniciando processamento de comandos para partida {MatchId}", matchId);

        try
        {
            await foreach (var command in reader.ReadAllAsync(cancellationToken))
            {
                await Task.Delay(2000);
                try
                {
                    Log.Debug("Executando comando {CommandType} para partida {MatchId}", 
                        command.GetType().Name, matchId);
                    
                    await command.Execute(this);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro ao executar comando {CommandType} para partida {MatchId}", 
                        command.GetType().Name, matchId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Processamento de comandos cancelado para partida {MatchId}", matchId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro fatal no processamento de comandos para partida {MatchId}", matchId);
        }
    }

    /// <summary>
    /// Encerra uma partida e limpa seus recursos
    /// </summary>
    public Task<bool> EndMatch(string matchId)
    {
        if (_matchChannels.TryRemove(matchId, out var channel))
        {
            channel.Writer.Complete();
            
            if (_matchCancellationTokens.TryRemove(matchId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }

            Log.Information("Partida {MatchId} encerrada", matchId);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Retorna o número de partidas ativas
    /// </summary>
    public int GetActiveMatchesCount() => _matchChannels.Count;

    /// <summary>
    /// Retorna os IDs de todas as partidas ativas
    /// </summary>
    public IEnumerable<string> GetActiveMatchIds() => _matchChannels.Keys;

    /// <summary>
    /// Obtém o estado atual de uma partida
    /// </summary>
    public Api.MatchStateDto? GetMatchState(string matchId)
    {
        if (!TrucoService.Matches.TryGetValue(matchId, out var match))
        {
            return null;
        }

        var isFinished = TrucoService.IsOver;
        string? winnerTeam = null;
        if (isFinished)
        {
            if (TrucoService.MatchScoreTeamA >= 12)
                winnerTeam = "TeamA";
            else if (TrucoService.MatchScoreTeamB >= 12)
                winnerTeam = "TeamB";
        }
        
        var state = new Api.MatchStateDto
        {
            MatchId = matchId,
            MatchScoreTeamA = TrucoService.MatchScoreTeamA,
            MatchScoreTeamB = TrucoService.MatchScoreTeamB,
            TurnScoreTeamA = TrucoService.TurnScoreTeamA,
            TurnScoreTeamB = TrucoService.TurnScoreTeamB,
            CurrentReward = match.Reward,
            State = match.State.ToString(),
            CurrentTurn = match.Turn,
            CurrentPlayerId = match.TurnOrder.Count > 0 ? match.TurnOrder.First?.Value : null,
            IsFinished = isFinished,
            WinnerTeam = winnerTeam,
            Manilha = match.Manilha != null ? new Api.CardDto
            {
                Name = match.Manilha.Name,
                Suit = match.Manilha.Suit,
                Rank = match.Manilha.Rank,
                SuitRank = match.Manilha.SuitRank,
                Hide = false
            } : null,
            Comments = TrucoService.GetMatchComments(matchId)
        };

        // Mapear jogadores - intercalando times para posições visuais corretas
        // Position 0 = TeamA[0] (Top), 1 = TeamB[0] (Right), 2 = TeamA[1] (Bottom), 3 = TeamB[1] (Left)
        var playerPositionMap = new List<Truco.Models.Player>
        {
            match.TeamA[0], // Position 0 - Top
            match.TeamB[0], // Position 1 - Right
            match.TeamA[1], // Position 2 - Bottom
            match.TeamB[1]  // Position 3 - Left
        };
        
        for (int i = 0; i < playerPositionMap.Count; i++)
        {
            var player = playerPositionMap[i];
            // Get last comment from this player from MatchComments
            var allComments = TrucoService.GetMatchComments(matchId);
            var lastComment = allComments
                .Where(c => c.Contains($"(P{player.Id})"))
                .LastOrDefault();

            state.Players.Add(new Api.PlayerStateDto
            {
                Id = player.Id,
                Name = player.Name,
                Position = i, // 0=top, 1=right, 2=bottom, 3=left
                Hand = player.Hand.Select(card => new Api.CardDto
                {
                    Name = card.Name,
                    Suit = card.Suit,
                    Rank = card.Rank,
                    SuitRank = card.SuitRank,
                    Hide = card.Hide
                }).ToList(),
                LastComment = lastComment
            });
        }

        // Mapear cartas jogadas
        foreach (var action in match.PlayedCards)
        {
            if (action.Data is Truco.Models.Card card)
            {
                var player = playerPositionMap.FirstOrDefault(p => p.Id == action.PlayerId);
                state.PlayedCards.Add(new Api.PlayedCardDto
                {
                    PlayerId = action.PlayerId,
                    PlayerName = player?.Name ?? "Unknown",
                    Card = new Api.CardDto
                    {
                        Name = card.Name,
                        Suit = card.Suit,
                        Rank = card.Rank,
                        SuitRank = card.SuitRank,
                        Hide = card.Hide
                    },
                    Comment = action.Comment
                });
            }
        }

        return state;
    }
}

