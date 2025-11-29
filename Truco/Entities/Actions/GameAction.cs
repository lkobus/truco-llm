namespace truco_net.Truco.Entities.Actions
{
    public class GameAction
    {
        public InGameActionsEnum ActionType { get; set; }
        public int PlayerId { get; set; }
        public object? Data { get; set; }
        public string? Comment { get; set; } // Trash talk comment (max 5 words)
    }


    public enum InGameActionsEnum
    {
        PlayCard = 1,
        CallTruco = 2,
        AcceptTruco = 3,
        DeclineTruco = 4,
        RaiseTruco = 5,
        SkipTurn = 6        
    }
}