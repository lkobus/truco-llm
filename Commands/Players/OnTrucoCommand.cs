
using truco_net.Truco.Entities.Actions;
using truco_net.Truco.Models;
using truco_net.Truco.Entities.Players;

namespace truco_net.Commands.Players;

public class OnTrucoCommand : ICommand
{

    public string MatchId { get; set; } = string.Empty;
    public Match Match { get; set; }
    public Player Requester { get; set; }
    public Player Receiver { get; set; }
    public int Value { get; set; }
    private readonly TrucoNet.Infrastructure.CommentQueue? _commentQueue;


    public OnTrucoCommand(string matchid, Match match, Player requester, Player receiver, int value, TrucoNet.Infrastructure.CommentQueue? commentQueue = null)
    {
        MatchId = matchid;
        Match = match;
        Requester = requester;
        Receiver = receiver;
        Value = value;
        _commentQueue = commentQueue;
    }

    public async Task Execute(Mediator mediator)
    {
        await Task.Delay(1000);
        // Configura o TrucoService estático e matchId para players LLM/Gemini
        Player.TrucoServiceInstance = mediator.TrucoService;
        if (Receiver is LLMPlayer llmPlayer)
        {
            llmPlayer.CurrentMatchId = MatchId;
        }
        else if (Receiver is GeminiPlayer geminiPlayer)
        {
            geminiPlayer.CurrentMatchId = MatchId;
        }

        var actions = mediator.TrucoService.GetAvailableActions(MatchId, Receiver.Id, Match.Turn);
        var action = Receiver.OnTruco(
            Match,            
            actions,        
            mediator.TrucoService.GetCurrentBet(MatchId)       
        );

        // Adiciona o comentário à lista se existir
        if (!string.IsNullOrWhiteSpace(action.Comment))
        {
            mediator.TrucoService.AddComment(MatchId, Receiver.Id, Receiver.Name, action.Comment, action.ActionType.ToString());
            
            // Envia para a fila de comentários
            _commentQueue?.Enqueue(MatchId, Receiver.Id, action.Comment, action.ActionType.ToString());
        }

        switch(action.ActionType)
        {
            case InGameActionsEnum.AcceptTruco:
                mediator.TrucoService.AcceptTruco(MatchId);
                await mediator.EnqueueCommand(MatchId, new PlayerCommand(
                Match.TeamA.Concat(Match.TeamB)
                    .FirstOrDefault(p => p.Id == Match.TurnOrder.First.Value),                         
                Match, MatchId, mediator.CommentQueue)
                ); 
                return;                                 
            case InGameActionsEnum.DeclineTruco:
                mediator.TrucoService.Decline(MatchId, Receiver.Id);
                await mediator.EnqueueCommand(MatchId, new StartMatchCommand(
                    MatchId,
                    Match.TeamA,
                    Match.TeamB,
                    mediator.TrucoService.Matches[MatchId].GetNextPlayerStartingFrom(
                        mediator.TrucoService.NextStartPlayer.Value)
                ));                
                return;
            case InGameActionsEnum.RaiseTruco:
                mediator.TrucoService.AcceptTruco(MatchId);
                mediator.TrucoService.RaiseBet(MatchId, Receiver.Id);
                await mediator.EnqueueCommand(MatchId, new OnTrucoCommand(
                    MatchId,
                    Match,
                    Receiver,
                    Requester,                        
                    Match.Reward,
                    mediator.CommentQueue
                ));                
                return;
        }

        throw new NotImplementedException();
    }

}
