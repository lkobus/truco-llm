using truco_net.Truco.Entities.Actions;
using truco_net.Truco.Models;

namespace truco_net.Truco.Entities.Players;

public class RandomCardPlayer : Player
{
    public RandomCardPlayer(int id, string name) : base(id, name)
    {
    }

    private Card PlayCard(Random random, bool hide = false)
    {
        int cardIndex = random.Next(Hand.Count);
        Card playedCard = Hand[cardIndex];
        Hand.RemoveAt(cardIndex);
        playedCard.Hide = hide;
        return playedCard;
    }

    public override GameAction Play(Match match, List<InGameActionsEnum> availableActions)
    {
        if (availableActions == null || availableActions.Count == 0)
            throw new InvalidOperationException("No available actions.");

        var random = new Random();
        
        // Escolhe randomicamente uma das ações disponíveis
        var chosenAction = availableActions[random.Next(availableActions.Count)];
        
        switch (chosenAction)
        {
            case InGameActionsEnum.PlayCard:
                if (Hand.Count == 0)
                    throw new InvalidOperationException("No cards to play.");
                                                
                return new GameAction
                {
                    ActionType = InGameActionsEnum.PlayCard,
                    PlayerId = this.Id,
                    Data = PlayCard(random)
                };
            
            case InGameActionsEnum.CallTruco:                
                return new GameAction
                {
                    ActionType = InGameActionsEnum.CallTruco,
                    PlayerId = this.Id,
                    Data = null
                };            
            case InGameActionsEnum.SkipTurn:
                Console.WriteLine($"Player {Name} skipped turn.");
                if (Hand.Count == 0)
                    throw new InvalidOperationException("No cards to play.");                                

                return new GameAction
                {
                    ActionType = InGameActionsEnum.SkipTurn,
                    PlayerId = this.Id,
                    Data = PlayCard(random, hide: true)
                };
            
            default:
                throw new InvalidOperationException($"Action {chosenAction} not supported in Play method.");
        }
    }

    public override GameAction OnTruco(Match match, List<InGameActionsEnum> availableActions, int currentBet)
    {
        var rng = new Random();
        int decision = rng.Next(0, 3); // 0 = decline, 1 = accept
        if (decision == 1 && availableActions.Contains(InGameActionsEnum.AcceptTruco))
        {
            Console.WriteLine($"Player {Name} accepted Truco.");
            return new GameAction
            {
                ActionType = InGameActionsEnum.AcceptTruco,
                PlayerId = this.Id,
                Data = null,
            };
        }
        else if (availableActions.Contains(InGameActionsEnum.DeclineTruco))
        {
            Console.WriteLine($"Player {Name} declined Truco.");
            return new GameAction
            {
                ActionType = InGameActionsEnum.DeclineTruco,
                PlayerId = this.Id,
                Data = null,
            };
        }
        
        Console.WriteLine($"Player {Name} raised Truco.");
        return new GameAction
        {
            ActionType = InGameActionsEnum.RaiseTruco,
            PlayerId = this.Id,
            Data = null,
        };
       
    }

}
