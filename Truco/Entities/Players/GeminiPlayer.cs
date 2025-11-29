using System.Text;
using Mscc.GenerativeAI;
using truco_net.Truco.Entities.Actions;
using truco_net.Truco.Models;

namespace truco_net.Truco.Entities.Players;

public class GeminiPlayer : Player
{
    private readonly string _apiKey;
    private readonly GoogleAI _googleAI;
    private readonly GenerativeModel _model;
    public string CurrentMatchId { get; set; } = "";

    public GeminiPlayer(int id, string name, string apiKey) : base(id, name)
    {
        _apiKey = apiKey;
        _googleAI = new GoogleAI(_apiKey);
        _model = _googleAI.GenerativeModel(model: "gemini-2.0-flash-exp");
    }

    private string BuildGameContext(Match match, List<string> matchComments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== GAME STATE ===");
        sb.AppendLine($"Current Reward: {match.Reward} points");
        sb.AppendLine($"Turn: {match.Turn}");
        sb.AppendLine($"Match State: {match.State}");
        
        if (match.Manilha != null)
        {
            sb.AppendLine($"Manilha: {match.Manilha.Name}");
        }

        if (matchComments != null && matchComments.Any())
        {
            sb.AppendLine("\n=== MATCH CONVERSATION HISTORY ===");
            sb.AppendLine("Recent trash talk and actions from this match:");
            foreach (var comment in matchComments.TakeLast(10)) // Últimos 10 comentários
            {
                sb.AppendLine($"  {comment}");
            }
        }

        sb.AppendLine("\n=== YOUR HAND ===");
        for (int i = 0; i < Hand.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {Hand[i].Name} (Suit: {Hand[i].Suit}, Rank: {Hand[i].Rank})");
        }

        sb.AppendLine("\n=== PLAYED CARDS THIS ROUND (ON TABLE) ===");
        if (match.PlayedCards.Any())
        {
            foreach (var action in match.PlayedCards)
            {
                if (action.ActionType == InGameActionsEnum.PlayCard && action.Data is Card card)
                {
                    if (card.Hide)
                    {
                        sb.AppendLine($"Player {action.PlayerId}: [Hidden Card]");
                    }
                    else
                    {
                        sb.AppendLine($"Player {action.PlayerId}: {card.Name}");
                    }
                }
            }
        }
        else
        {
            sb.AppendLine("No cards on table yet.");
        }

        if (match.PreviousCards.Any())
        {
            sb.AppendLine("\n=== PREVIOUS ROUNDS HISTORY ===");
            int roundNum = 1;
            var groupedCards = match.PreviousCards
                .Where(a => a.ActionType == InGameActionsEnum.PlayCard)
                .Select((action, index) => new { action, index })
                .GroupBy(x => x.index / 4); // Agrupa de 4 em 4 (cada rodada)

            foreach (var round in groupedCards)
            {
                sb.AppendLine($"\nRound {roundNum++}:");
                foreach (var item in round)
                {
                    if (item.action.Data is Card card)
                    {
                        if (card.Hide)
                        {
                            sb.AppendLine($"  Player {item.action.PlayerId}: [Hidden Card]");
                        }
                        else
                        {
                            sb.AppendLine($"  Player {item.action.PlayerId}: {card.Name}");
                        }
                    }
                }
            }
        }

        return sb.ToString();
    }

    private string BuildAvailableActionsPrompt(List<InGameActionsEnum> availableActions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n=== AVAILABLE ACTIONS ===");
        
        foreach (var action in availableActions)
        {
            sb.AppendLine($"- {action}");
        }

        sb.AppendLine("\n=== STRATEGIC COMMENT ===");
        sb.AppendLine("Add a strategic comment with trash talk in PT-BR with 10-20 words.");
        sb.AppendLine("Be intimidate and strategic based on the conversation history!");      
        return sb.ToString();
    }

    private async Task<GameAction> GetGeminiDecision(string systemPrompt, string userPrompt, List<InGameActionsEnum> availableActions)
    {
        var prompt = $"{systemPrompt}\n\n{userPrompt}";
        
        var response = await _model.GenerateContent(prompt);
        var responseText = response.Text?.Trim() ?? "";

        Console.WriteLine($"Gemini Response: {responseText}");

        // Parse the response to determine the action
        return ParseGeminiResponse(responseText, availableActions);
    }

    private string ExtractComment(string response)
    {
        // Extract comment from patterns like "COMMENT: xyz" or "Comment: xyz"
        var commentMatch = System.Text.RegularExpressions.Regex.Match(
            response, 
            @"(?:COMMENT|Comment|comment|Comentário|comentário):\s*(.+?)(?:\n|$)", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        
        if (commentMatch.Success)
        {
            var comment = commentMatch.Groups[1].Value.Trim();
            // Remove the word "comentario" or "comentário" if present
            comment = System.Text.RegularExpressions.Regex.Replace(comment, @"\bcoment[aá]rio\b", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            // Remove quotes if present
            comment = comment.Trim('"', '\'');
            // Limit to 20 words
            var words = comment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words.Take(20));
        }

        // Try to get last line as comment
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 1)
        {
            var lastLine = lines[^1].Trim();
            // Check if it looks like a comment (not an action)
            if (!lastLine.Contains("PlayCard", StringComparison.OrdinalIgnoreCase) &&
                !lastLine.Contains("CallTruco", StringComparison.OrdinalIgnoreCase) &&
                !lastLine.Contains("SkipTurn", StringComparison.OrdinalIgnoreCase) &&
                !lastLine.Contains("Accept", StringComparison.OrdinalIgnoreCase) &&
                !lastLine.Contains("Decline", StringComparison.OrdinalIgnoreCase) &&
                !lastLine.Contains("Raise", StringComparison.OrdinalIgnoreCase) &&
                lastLine.Length > 5)
            {
                var comment = lastLine;
                // Remove the word "comentario" or "comentário" if present
                comment = System.Text.RegularExpressions.Regex.Replace(comment, @"\bcoment[aá]rio\b", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                // Remove quotes if present
                comment = comment.Trim('"', '\'');
                var words = comment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return string.Join(" ", words.Take(20));
            }
        }
        
        // Try to find any Portuguese text (seems like a comment)
        var portugueseMatch = System.Text.RegularExpressions.Regex.Match(
            response,
            @"([A-ZÀ-Ü][^\n]{10,100})",
            System.Text.RegularExpressions.RegexOptions.Multiline
        );
        
        if (portugueseMatch.Success)
        {
            var potentialComment = portugueseMatch.Groups[1].Value.Trim();
            // Make sure it's not an action line
            if (!potentialComment.Contains("PlayCard", StringComparison.OrdinalIgnoreCase) &&
                !potentialComment.Contains("CallTruco", StringComparison.OrdinalIgnoreCase) &&
                !potentialComment.Contains("SkipTurn", StringComparison.OrdinalIgnoreCase) &&
                !potentialComment.Contains("Accept", StringComparison.OrdinalIgnoreCase) &&
                !potentialComment.Contains("Decline", StringComparison.OrdinalIgnoreCase) &&
                !potentialComment.Contains("Raise", StringComparison.OrdinalIgnoreCase))
            {
                var comment = potentialComment;
                // Remove the word "comentario" or "comentário" if present
                comment = System.Text.RegularExpressions.Regex.Replace(comment, @"\bcoment[aá]rio\b", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                // Remove quotes if present
                comment = comment.Trim('"', '\'');
                var words = comment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return string.Join(" ", words.Take(20));
            }
        }

        return ""; // Empty comment if nothing found
    }

    private GameAction ParseGeminiResponse(string response, List<InGameActionsEnum> availableActions)
    {
        string comment = ExtractComment(response);
        Console.WriteLine($"Extracted Comment: '{comment}'");
        response = response.ToLower();

        // Try to identify the action from the response
        if (response.Contains("playcard") || response.Contains("play card"))
        {
            if (!availableActions.Contains(InGameActionsEnum.PlayCard))
                throw new InvalidOperationException("PlayCard action not available.");

            // Try to extract card index
            int cardIndex = ExtractCardIndex(response);
            if (cardIndex < 0 || cardIndex >= Hand.Count)
                cardIndex = 0; // Default to first card

            Card playedCard = Hand[cardIndex];
            Hand.RemoveAt(cardIndex);

            return new GameAction
            {
                ActionType = InGameActionsEnum.PlayCard,
                PlayerId = this.Id,
                Data = playedCard,
                Comment = comment
            };
        }
        else if (response.Contains("calltruco") || response.Contains("call truco"))
        {
            if (!availableActions.Contains(InGameActionsEnum.CallTruco))
                return GetDefaultAction(availableActions);

            return new GameAction
            {
                ActionType = InGameActionsEnum.CallTruco,
                PlayerId = this.Id,
                Data = null,
                Comment = comment
            };
        }
        else if (response.Contains("skipturn") || response.Contains("skip turn") || response.Contains("skip"))
        {
            if (!availableActions.Contains(InGameActionsEnum.SkipTurn))
                return GetDefaultAction(availableActions);

            int cardIndex = 0;
            Card playedCard = Hand[cardIndex];
            Hand.RemoveAt(cardIndex);
            playedCard.Hide = true;

            return new GameAction
            {
                ActionType = InGameActionsEnum.SkipTurn,
                PlayerId = this.Id,
                Data = playedCard,
                Comment = comment
            };
        }
        else if (response.Contains("accept") && availableActions.Contains(InGameActionsEnum.AcceptTruco))
        {
            return new GameAction
            {
                ActionType = InGameActionsEnum.AcceptTruco,
                PlayerId = this.Id,
                Data = null,
                Comment = comment
            };
        }
        else if (response.Contains("decline") && availableActions.Contains(InGameActionsEnum.DeclineTruco))
        {
            return new GameAction
            {
                ActionType = InGameActionsEnum.DeclineTruco,
                PlayerId = this.Id,
                Data = null,
                Comment = comment
            };
        }
        else if (response.Contains("raise") && availableActions.Contains(InGameActionsEnum.RaiseTruco))
        {
            return new GameAction
            {
                ActionType = InGameActionsEnum.RaiseTruco,
                PlayerId = this.Id,
                Data = null,
                Comment = comment
            };
        }

        // Fallback to default action
        return GetDefaultAction(availableActions);
    }

    private int ExtractCardIndex(string response)
    {
        // Try to find "card 1", "card 2", etc.
        var match = System.Text.RegularExpressions.Regex.Match(response, @"card\s*(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
        {
            return index - 1; // Convert to 0-based index
        }

        // Try to find just a number
        match = System.Text.RegularExpressions.Regex.Match(response, @"\b([123])\b");
        if (match.Success && int.TryParse(match.Groups[1].Value, out index))
        {
            return index - 1;
        }

        return 0; // Default to first card
    }

    private GameAction GetDefaultAction(List<InGameActionsEnum> availableActions)
    {
        if (availableActions.Contains(InGameActionsEnum.PlayCard))
        {
            if (Hand.Count == 0)
                throw new InvalidOperationException("No cards to play.");

            Card playedCard = Hand[0];
            Hand.RemoveAt(0);

            return new GameAction
            {
                ActionType = InGameActionsEnum.PlayCard,
                PlayerId = this.Id,
                Data = playedCard
            };
        }

        if (availableActions.Contains(InGameActionsEnum.DeclineTruco))
        {
            return new GameAction
            {
                ActionType = InGameActionsEnum.DeclineTruco,
                PlayerId = this.Id,
                Data = null
            };
        }

        throw new InvalidOperationException("No valid default action available.");
    }

    public override GameAction Play(Match match, List<InGameActionsEnum> availableActions)
    {
        Thread.Sleep(2000);
        if (availableActions == null || availableActions.Count == 0)
            throw new InvalidOperationException("No available actions.");

        string systemPrompt = @"
Você é um jogador especialista de Truco respondendo a uma jogada.

O guia de estratégia é

- Você só pede truco no primeiro round se tiver 1 ou mais manilhas e varios 3
- Se você é o primeiro da rodada não gaste sua carta mais forte
- Se você é o ultimo da rodada do seu time (round 3 ou 4) você precisa jogar cartas altas pra levar o round
- Use sempre a minima carta pra ganhar o turno
- Se você estiver perdendo não aceite truco.
- Você pode aumentar a aposta qunado sabe que vai ganhar a mão atual e ainda tem um carta forte sobrando

Lembre-se a manilha sempre deixar a carta do proximo rank mais fortes porém 
clubs > hearts > spades > diamonds

";

        var matchComments = new List<string>(); // Will be populated by PlayerCommand
        if (TrucoServiceInstance != null && !string.IsNullOrEmpty(CurrentMatchId))
        {
            matchComments = TrucoServiceInstance.GetMatchComments(CurrentMatchId);
        }
        string userPrompt = BuildGameContext(match, matchComments) + BuildAvailableActionsPrompt(availableActions);

        try
        {
            var action = GetGeminiDecision(systemPrompt, userPrompt, availableActions).Result;
            Console.WriteLine($"Player {Name} chose action: {action.ActionType}");
            return action;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Gemini decision: {ex.Message}");
            return GetDefaultAction(availableActions);
        }
    }

    public override GameAction OnTruco(Match match, List<InGameActionsEnum> availableActions, int currentBet)
    {
        if (availableActions == null || availableActions.Count == 0)
            throw new InvalidOperationException("No available actions.");

        string systemPrompt = @"You are an expert Truco player responding to a Truco call (bet raise).

Your options are:
- AcceptTruco: Accept the raised stakes
- DeclineTruco: Fold and give up the round (AVOID THIS!)
- RaiseTruco: Counter-raise the stakes even higher

CRITICAL STRATEGY:
- DEFAULT to AcceptTruco unless you have a TERRIBLE hand (all low cards, no Manilhas)
- Declining gives FREE points to opponents - only decline if you're certain you'll lose
- Even with mediocre cards, it's often worth accepting and playing strategically
- ONLY RaiseTruco if you have EXCELLENT cards (Manilha + high cards) AND current bet is not already high
- If bet is already 6+ points, think twice before raising - accept and play smart instead
- In rounds 1-2: Accept more often, only raise with very strong hands
- In round 3 (final): You can be more aggressive with raises if you're behind

Consider:
1. Your hand strength (Manilhas > High cards > Low cards)
2. Current bet value (3 points = low risk, 6+ = high risk)
3. Round number (early = conservative, final = can be aggressive)
4. Match score (if losing badly, take more risks)

Pay attention to the conversation history to maintain context and respond appropriately.
Respond with the action name on the first line.
Then add a STRATEGIC comment (10-20 words) in PT-BR on a new line starting with 'Comment:'.

IMPORTANT for the comment:
- React to who called the truco and the match context
- Use psychological tactics to create doubt
- Reference the conversation history if relevant
- DO NOT just describe your action

Example responses:
AcceptTruco
Comment: Vocês tão achando que vou correr é? Pode vir que aguento

AcceptTruco
Comment: Aceito tranquilo, vamos ver quem tem carta boa de verdade

RaiseTruco
Comment: Trucou foi pouco hein, bora subir mais que tô confiante";

        var matchComments = new List<string>(); // Will be populated by PlayerCommand
        if (TrucoServiceInstance != null && !string.IsNullOrEmpty(CurrentMatchId))
        {
            matchComments = TrucoServiceInstance.GetMatchComments(CurrentMatchId);
        }
        string userPrompt = BuildGameContext(match, matchComments) + 
                          $"\n\nCurrent Bet: {currentBet} points\n" +
                          BuildAvailableActionsPrompt(availableActions);

        try
        {
            var action = GetGeminiDecision(systemPrompt, userPrompt, availableActions).Result;
            Console.WriteLine($"Player {Name} responded to Truco with: {action.ActionType}");
            return action;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Gemini decision for Truco: {ex.Message}");
            return GetDefaultAction(availableActions);
        }
    }
}
