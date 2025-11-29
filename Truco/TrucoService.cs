

using truco_net.Commands;
using truco_net.Truco.Entities.Actions;
using truco_net.Truco.Entities.Matches;
using truco_net.Truco.Models;

namespace truco_net.Truco;

public class TrucoService
{
    public Dictionary<string, Match> Matches { get; } = new Dictionary<string, Match>();
    public Dictionary<string, List<string>> MatchComments { get; } = new Dictionary<string, List<string>>();
    public int MatchScoreTeamA { get; set; } = 0;
    public int MatchScoreTeamB { get; set; } = 0;    
    public int TurnScoreTeamA { get; set; } = 0;
    public int TurnScoreTeamB { get; set; } = 0;
    public int? StartRoundPlayerId { get; set; } = null;    
    public int? NextStartPlayer {get; set;} = null;

    public bool IsOver => MatchScoreTeamA >= 12 || MatchScoreTeamB >= 12;
    public bool IsMatchFinished => 
        (TurnScoreTeamA >= 2 || 
        TurnScoreTeamB >= 2) && TurnScoreTeamA + TurnScoreTeamB != 4;

    public int GetCurrentPlayerId(string matchId)
    {
        var match = Matches[matchId];
        return match.TurnOrder.First.Value;
    }    

    public void StartMatch(string matchId, List<Player> teamA, List<Player> teamB, int? startRoundPlayer)
    {
        TurnScoreTeamA = 0;
        TurnScoreTeamB = 0;
        if(Matches.ContainsKey(matchId))
            Matches.Remove(matchId);
        
        // Inicializa ou reseta a lista de comentários da partida
        if(MatchComments.ContainsKey(matchId))
            MatchComments[matchId].Clear();
        else
            MatchComments.Add(matchId, new List<string>());
        
        StartRoundPlayerId = startRoundPlayer;
        var match = new Match(teamA, teamB);        
        if(NextStartPlayer == null)
            NextStartPlayer = (int)StartRoundPlayerId;
        else
            NextStartPlayer = match.GetNextPlayerStartingFrom((int)NextStartPlayer);
        match.Reward = 1;
        match.State = StateEnum.FIRST_MOVE;
        match.LastTrucoCallerId = null;
        Matches.Add(matchId.ToString(), match);
        var deck = new TrucoDeck();
        deck.Shuffle();
        match.TeamA.Concat(match.TeamB).ToList().ForEach(player => player.Hand.Clear());
        match.Manilha = deck.Draw();
        match.SetStarterPlayer((int)startRoundPlayer);
        Console.WriteLine($"Manilha is {match.Manilha.Name} of {match.Manilha.Suit}");
        LoadHands(match, deck);
    }

    

    public void SendCardToDesk(Match match, GameAction action)
    {            
        match.PlayedCards.Add(action);        
        match.TurnOrder.RemoveFirst();
        match.TurnOrder.AddLast(action.PlayerId);
        match.Turn++;

        if(match.Turn > 0)
            match.State = StateEnum.WAITING_MOVE;
    }

    public void AddComment(string matchId, int playerId, string playerName, string comment, string actionType)
    {
        if (!string.IsNullOrWhiteSpace(comment) && MatchComments.ContainsKey(matchId))
        {
            var commentEntry = $"[{playerName} (P{playerId})]: {comment} ({actionType})";
            MatchComments[matchId].Add(commentEntry);
        }
    }

    public List<string> GetMatchComments(string matchId)
    {
        return MatchComments.ContainsKey(matchId) ? MatchComments[matchId] : new List<string>();
    }

    private void LoadHands(Match match, TrucoDeck deck)
    {                
        for (int i = 0; i < 3; i++)
        {
            match.TeamA.Concat(match.TeamB).ToList().ForEach(player =>
            {                
                player.ReceiveCard(deck.Draw());
            });
        }        
    }

    public List<InGameActionsEnum> GetAvailableActions(string matchId, int playerId, int round)
    {
        var match = Matches[matchId];
        var currentPlayerTeam = match.TeamA.FirstOrDefault(p => p.Id == playerId) != null ? "A" : "B";
        var lastCallerTeam = match.LastTrucoCallerId.HasValue 
            ? (match.TeamA.FirstOrDefault(p => p.Id == match.LastTrucoCallerId) != null ? "A" : "B")
            : null;
        
        switch(match.State)
        {
            case Entities.Matches.StateEnum.FIRST_MOVE:                
                return new List<InGameActionsEnum>
                {
                    InGameActionsEnum.PlayCard,
                    InGameActionsEnum.CallTruco
                };                                        
            case Entities.Matches.StateEnum.WAITING_MOVE:
                var actions = new List<InGameActionsEnum>
                {
                    InGameActionsEnum.PlayCard,                    
                    InGameActionsEnum.SkipTurn
                };
                
                // Só pode pedir truco se:
                // 1. Ainda não pediu truco nesta rodada (lastCallerTeam == null)
                // 2. OU o outro time foi quem pediu por último (lastCallerTeam != currentPlayerTeam)
                if (lastCallerTeam == null || lastCallerTeam != currentPlayerTeam)
                {
                    actions.Add(InGameActionsEnum.CallTruco);
                }
                
                return actions;    
            case Entities.Matches.StateEnum.TRUCO:
                var trucoActions = new List<InGameActionsEnum>
                {
                    InGameActionsEnum.AcceptTruco,
                    InGameActionsEnum.DeclineTruco
                };
                
                // Só pode aumentar se a aposta atual não for 12
                if (match.Reward < 12)
                {
                    trucoActions.Add(InGameActionsEnum.RaiseTruco);
                }
                
                return trucoActions;            
                
            case Entities.Matches.StateEnum.TRUCO_ACCEPTED:
                var acceptedActions = new List<InGameActionsEnum>
                {
                    InGameActionsEnum.PlayCard
                };
                
                // Após aceitar, apenas o time que NÃO pediu pode aumentar novamente
                if (lastCallerTeam != null && lastCallerTeam != currentPlayerTeam && match.Reward < 12)
                {
                    acceptedActions.Add(InGameActionsEnum.CallTruco);
                }
                
                return acceptedActions;    
                
        }
        throw new NotImplementedException();   
    }

    public void Decline(string matchid, int playerId)
    {
        var match = Matches[matchid];
        match.State = StateEnum.DECLINE;
        
        // Quando recusa, o time adversário ganha o valor ATUAL do reward
        // (não o próximo valor, pois o truco não foi aceito)
        if(match.TeamA.FirstOrDefault(p => p.Id == playerId) != null)
        {
            TurnScoreTeamB = 2;
            MatchScoreTeamB += match.Reward;
        }            
        else
        {
            MatchScoreTeamA += match.Reward;
            TurnScoreTeamA = 2;
        }
            
        
        
        Console.WriteLine($"Player {playerId} declined Truco. Match score: Team A {MatchScoreTeamA} x Team B {MatchScoreTeamB}");
    }

    public int GetCurrentBet(string matchid)
    {
        var match = Matches[matchid];
        switch(match.Reward)
        {
            case 1:
                return 3;
            case 3:
                return 6;
            case 6:
                return 9;
            default:
                return 12;
        }        
    }    

    public void RaiseBet(string matchId, int playerId)
    {
        var match = Matches[matchId];
        // NÃO aumenta o reward aqui! Só muda o estado e registra quem pediu
        // O reward só aumenta quando o truco é aceito em AcceptTruco
        match.State = StateEnum.TRUCO;
        match.LastTrucoCallerId = playerId;
    }

    public bool IsManilha(Card card, Card manilha)
    {
        if(manilha.Rank == 10) 
            return card.Rank == 1;

        return card.Rank == manilha.Rank + 1;
    }

    public bool IsDraw(List<GameAction> playedCards, Match match)
    {
        var highScore = playedCards.Last().Data as Card;
        var count = playedCards.Count(p => p.Data is Card card && card.Rank == highScore.Rank);
        return count > 1 && !IsManilha(highScore, match.Manilha);
    }

    public void AcceptTruco(string matchId)
    {
        var match = Matches[matchId];
        // Aqui sim aumenta o reward para o próximo valor
        match.Reward = GetCurrentBet(matchId);
        match.State = StateEnum.TRUCO_ACCEPTED;
        Console.WriteLine($"Truco accepted! Current bet is now {match.Reward} points.");
        // Mantém o LastTrucoCallerId para controlar quem pode aumentar
    }

    public void CloseMatch(Match match, string lobby)
    {                
        match.PlayedCards.Sort((card1, card2) => 
        {
            
            var data = card1.Data as Card;
            var data2 = card2.Data as Card;
            
            var data1rank = data.Rank;
            var data2rank = data2.Rank;

            var manilhaPenalty = 0;            

            if(IsManilha(data, match.Manilha))
                data1rank = 50 + data.SuitRank;
            
            if(IsManilha(data2, match.Manilha))
                data2rank = 50 + data2.SuitRank;

            if (data == null || data2 == null)
                return 0;
            return data1rank.CompareTo(data2rank); 
        });        
        bool? winner;
        var winningCard = match.PlayedCards.Where(p => !(p.Data as Card).Hide).Last();
        
        if (IsDraw(match.PlayedCards, match))
        {
            TurnScoreTeamA++;
            TurnScoreTeamB++;
            Console.WriteLine($"Round Draw! Turn score: Team A {TurnScoreTeamA} x Team B {TurnScoreTeamB}");
            
            match.PreviousCards.AddRange(match.PlayedCards);
            match.PlayedCards.Clear();
            match.SetStarterPlayer((int)StartRoundPlayerId);
            match.Turn = 0;            
            
            // Verifica se algum time ganhou o turno completo após o empate
            if(TurnScoreTeamA == TurnScoreTeamB)
            {                
                return;
            }
            if(TurnScoreTeamA >= 2)
            {
                MatchScoreTeamA += match.Reward;                
            }
            else if(TurnScoreTeamB >= 2)
            {
                MatchScoreTeamB += match.Reward;
            }
            
            Console.WriteLine($"Match score: Team A {MatchScoreTeamA} x Team B {MatchScoreTeamB}");
            
            if(MatchScoreTeamA >= 12 || MatchScoreTeamB >= 12)
                Console.WriteLine($"Match finished! Final score: Team A {MatchScoreTeamA} x Team B {MatchScoreTeamB}");
        }
        else
        {
            if (match.TeamA.FirstOrDefault( p => p.Id == winningCard.PlayerId) != null)
                winner = true;
            else
                winner = false;

            Console.WriteLine($"Round winner: Team {(winner == true ? "A" : "B")}");
            
            if(winner == true)
                TurnScoreTeamA++;
            else
                TurnScoreTeamB++;
            
            match.PreviousCards.AddRange(match.PlayedCards);
            match.PlayedCards.Clear();
            match.SetStarterPlayer((int)winningCard.PlayerId);
            match.Turn = 0;            
            
            Console.WriteLine($"Turn score: Team A {TurnScoreTeamA} x Team B {TurnScoreTeamB}");
            
            // Verifica se algum time ganhou o turno completo (melhor de 3)
            if(TurnScoreTeamA >= 2)
            {
                MatchScoreTeamA += match.Reward;                
            }
            else if(TurnScoreTeamB >= 2)
            {
                MatchScoreTeamB += match.Reward;                                
            }
            
            Console.WriteLine($"Match score: Team A {MatchScoreTeamA} x Team B {MatchScoreTeamB}");
            
            if(MatchScoreTeamA >= 12 || MatchScoreTeamB >= 12)
                Console.WriteLine($"Match finished! Final score: Team A {MatchScoreTeamA} x Team B {MatchScoreTeamB}");
        }
        
        
    }
}
