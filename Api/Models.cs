namespace truco_net.Api;

public class CreateMatchRequest
{
    public string MatchId { get; set; } = string.Empty;
    public List<string> Players { get; set; } = new();
}

public class StartMatchRequest
{
    public string? MatchId { get; set; } // Opcional - será gerado se não fornecido
    public List<PlayerDto> TeamA { get; set; } = new();
    public List<PlayerDto> TeamB { get; set; } = new();
    public int StartRoundPlayer { get; set; } = 1;
}

public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "RandomCardPlayer"; // RandomCardPlayer, LLMPlayer, GeminiPlayer, etc.
    public string? ApiKey { get; set; } // Obrigatório apenas para LLMPlayer e GeminiPlayer
}

public class CommandRequest
{
    public string MatchId { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class MatchStateDto
{
    public string MatchId { get; set; } = string.Empty;
    public int MatchScoreTeamA { get; set; }
    public int MatchScoreTeamB { get; set; }
    public int TurnScoreTeamA { get; set; }
    public int TurnScoreTeamB { get; set; }
    public int CurrentReward { get; set; }
    public string State { get; set; } = string.Empty;
    public CardDto? Manilha { get; set; }
    public List<PlayerStateDto> Players { get; set; } = new();
    public List<PlayedCardDto> PlayedCards { get; set; } = new();
    public List<string> Comments { get; set; } = new(); // Todos os comentários da partida
    public int CurrentTurn { get; set; }
    public int? CurrentPlayerId { get; set; }
    public bool IsFinished { get; set; } // Se a partida terminou (alguém chegou a 12)
    public string? WinnerTeam { get; set; } // "TeamA", "TeamB" ou null se não terminou
}

public class PlayerStateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; } // 0=top, 1=right, 2=bottom, 3=left
    public List<CardDto> Hand { get; set; } = new();
    public string? LastComment { get; set; } // Last trash talk comment
}

public class CardDto
{
    public string Name { get; set; } = string.Empty;
    public string Suit { get; set; } = string.Empty;
    public int Rank { get; set; }
    public int SuitRank { get; set; }
    public bool Hide { get; set; }
}

public class PlayedCardDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public CardDto Card { get; set; } = new();
    public string? Comment { get; set; } // Trash talk comment from player
}
