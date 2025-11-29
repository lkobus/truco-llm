using Serilog;
using truco_net.Commands.Players;
using truco_net.Truco.Models;

namespace truco_net.Commands;

public class StartMatchCommand : ICommand
{
    public string MatchId { get; set; } = string.Empty;
    public List<Player> TeamA {get; }
    public List<Player> TeamB {get;}
    public int StartRoundPlayer { get; }

    public StartMatchCommand(string matchId, List<Player> teamA, List<Player> teamB, int startRoundPlayer = 1)
    {
        StartRoundPlayer = startRoundPlayer;
        MatchId = matchId;
        TeamA = teamA;
        TeamB = teamB;
    }

    public async Task Execute(Mediator mediator)
    {
        await Task.Delay(3000); //wait 3 seconds before starting a new match
        mediator.TrucoService.StartMatch(MatchId, TeamA, TeamB, StartRoundPlayer);
        await mediator.EnqueueCommand(MatchId, new PlayerCommand( 
            TeamA.Concat(TeamB)
            .FirstOrDefault(p => p.Id == StartRoundPlayer),
            mediator.TrucoService.Matches[MatchId],
            MatchId, mediator.CommentQueue));
    }
}
