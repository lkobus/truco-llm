namespace truco_net.Truco.Models
{
    public class Card
    {    
        public string Name { get; set; } // e.g., "Ace of Spades"
        public string Suit { get; set; } // "Hearts", "Diamonds", "Clubs", "Spades"
        public int SuitRank { get; set; } // 1=Hearts, 2=Diamonds, 3=Clubs, 4=Spades
        public int Rank { get; set; } // 1,2,3,4,5,6,7
        public bool Hide {get; set; } = false;
    }
}