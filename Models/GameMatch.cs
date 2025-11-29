namespace truco_net.Models;

public class GameMatch
{
    public string MatchId { get; set; } = string.Empty;
    public List<string> Players { get; set; } = new();
    public string Status { get; set; } = "Waiting";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
