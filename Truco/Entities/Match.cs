using truco_net.Truco.Entities.Actions;
using truco_net.Truco.Entities.Matches;

namespace truco_net.Truco.Models;

public class Match
{
    public string Id {get;set;}
    public List<Player> TeamA {get; }
    public List<Player> TeamB {get;}
    public LinkedList<int> TurnOrder { get; set; } = new LinkedList<int>();
    public bool? Winner { get; }     
    public List<GameAction> PlayedCards { get; }
    public List<GameAction> PreviousCards {get; }
    public int Turn { get; set; } = 0;
    public int Reward { get; set; } = 1;
    public StateEnum State { get; set; }
    public Card? Manilha { get; set; }
    public int? LastTrucoCallerId { get; set; } // Player ID who called the last truco/raise

    public bool LastMove => Turn == 4;
    public Match(List<Player> teamA, List<Player> teamB)
    {        
        PreviousCards = new List<GameAction>();
        TurnOrder = new LinkedList<int>(new int[] {teamA[0].Id, teamB[0].Id, teamA[1].Id, teamB[1].Id});        
        TeamA = teamA;
        State = StateEnum.FIRST_MOVE;
        TeamB = teamB;        
        Winner = null;        
        PlayedCards = new List<GameAction>();
    }

    public void SetStarterPlayer(int playerId)
    {
        while(TurnOrder.First.Value != playerId)
        {
            var first = TurnOrder.First.Value;
            TurnOrder.RemoveFirst();
            TurnOrder.AddLast(first);
        }
    }

    public int GetNextPlayerStartingFrom(int playerId)
    {
        var node = TurnOrder.Find(playerId);
        if (node == null || node.Next == null || node.Next.Value == null)
            return TurnOrder.First.Value;
        return node.Next.Value;
    }
}
