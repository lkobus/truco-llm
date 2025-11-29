using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using truco_net.Truco.Entities.Actions;

namespace truco_net.Truco.Models
{
    public abstract class Player
    {
        public int Id { get; }
        public string Name { get; }
        public List<Card> Hand { get;}
        public static TrucoService? TrucoServiceInstance { get; set; }

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
            Hand = new List<Card>();
        }

        public void ReceiveCard(Card card)
        {
            Hand.Add(card);
        }

        public virtual GameAction OnTruco(Match match, List<InGameActionsEnum> availableActions, int currentBet)
        {
            throw new NotImplementedException();   
        }

        public virtual GameAction Play(Match match, List<InGameActionsEnum> availableActions)
        {
            throw new NotImplementedException();   
        }
    }
}