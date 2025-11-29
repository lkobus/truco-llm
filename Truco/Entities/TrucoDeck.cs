using Xoshiro.PRNG32;

namespace truco_net.Truco.Models;

public class TrucoDeck
{
    private List<Card> _cards;

    public TrucoDeck()
    {
        _cards = new List<Card>();
        string[] cards = { "4", "5", "6", "7", "Q", "J", "K", "A", "2", "3" };
        string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
        int[] suitRanks = { 1, 2, 3, 4 }; // Corresponding ranks for suits

        
        foreach (var suit in suits)
        {
            int suitRank = Array.IndexOf(suits, suit) + 1;
            for (int i = 0; i < cards.Length; i++)
            {
                var card = new Card
                {
                    Name = $"{cards[i]}",
                    Suit = suit,
                    SuitRank = suitRank,
                    Rank = i + 1
                };
                _cards.Add(card);
            }
        }
    }

    public Card Draw()
    {
        if (_cards.Count == 0)
            return null;

        var card = _cards[0];
        _cards.RemoveAt(0);
        return card;
    }

    public void Shuffle()
    {
        var xor = new XoShiRo128plus();
        for (int i = _cards.Count - 1; i > 0; i--)
        {
            int j = xor.Next(0, i + 1);
            var temp = _cards[i];
            _cards[i] = _cards[j];
            _cards[j] = temp;
        }
    }
}
