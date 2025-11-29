using truco_net.Commands.Players;
using truco_net.Truco.Models;

namespace truco_net.Commands;

public class CloseMatchCommand : ICommand
{
    public string MatchId { get; set; } = string.Empty;
    private readonly Match _match;
    public CloseMatchCommand(string matchId, Match match)
    {
        _match = match;
        MatchId = matchId;
    }

    public async Task Execute(Mediator mediator)
    {
        if(mediator.TrucoService.Matches[MatchId].Turn == 4)
        {
            mediator.TrucoService.CloseMatch(mediator.TrucoService.Matches[MatchId], MatchId);
            
            // Verifica se a partida terminou (algum time chegou a 12 pontos)
            if(mediator.TrucoService.IsOver)
            {
                Console.WriteLine($"🏆 Match {MatchId} finished! Final score: Team A {mediator.TrucoService.MatchScoreTeamA} x Team B {mediator.TrucoService.MatchScoreTeamB}");
                // Não enfileira mais comandos - a partida acabou
                return;
            }
            
            if(!mediator.TrucoService.IsMatchFinished)
            {
                await mediator.EnqueueCommand(MatchId, new PlayerCommand(
                _match.TeamA.Concat(_match.TeamB)
                    .FirstOrDefault(p => p.Id == _match.TurnOrder.First.Value),                         
                _match, MatchId, mediator.CommentQueue)
                );
            } else
            {
                // Inicia nova rodada (não nova partida)
                await mediator.EnqueueCommand(MatchId, new StartMatchCommand(
                    MatchId,
                    _match.TeamA,
                    _match.TeamB,
                    mediator.TrucoService.Matches[MatchId].GetNextPlayerStartingFrom(
                        mediator.TrucoService.NextStartPlayer.Value)
                ));
            }
        }
            
        else
            await mediator.EnqueueCommand(MatchId, new PlayerCommand( 
                _match.TeamA.Concat(_match.TeamB)
                .FirstOrDefault(p => p.Id == _match.TurnOrder.First.Value),                         
            _match, MatchId, mediator.CommentQueue)
            );        
    }

}
