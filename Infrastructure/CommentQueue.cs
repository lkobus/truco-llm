using System.Collections.Concurrent;

namespace TrucoNet.Infrastructure;

/// <summary>
/// Fila thread-safe para gerenciar comentários pendentes de exibição
/// </summary>
public class CommentQueue
{
    private readonly ConcurrentQueue<CommentMessage> _queue = new();
    
    public void Enqueue(string matchId, int playerId, string comment, string action)
    {
        if (string.IsNullOrWhiteSpace(comment) || comment == "...")
            return;
            
        _queue.Enqueue(new CommentMessage
        {
            MatchId = matchId,
            PlayerId = playerId,
            Comment = comment,
            Action = action,
            Timestamp = DateTime.UtcNow
        });
    }
    
    public List<CommentMessage> DequeueAll(string matchId)
    {
        var comments = new List<CommentMessage>();
        
        while (_queue.TryPeek(out var message))
        {
            if (message.MatchId == matchId)
            {
                if (_queue.TryDequeue(out var dequeued))
                {
                    comments.Add(dequeued);
                }
            }
            else
            {
                // Se não é desta partida, para de processar
                break;
            }
        }
        
        return comments;
    }
    
    public void Clear(string matchId)
    {
        var temp = new List<CommentMessage>();
        
        // Remove todos os comentários desta partida
        while (_queue.TryDequeue(out var message))
        {
            if (message.MatchId != matchId)
            {
                temp.Add(message);
            }
        }
        
        // Re-adiciona comentários de outras partidas
        foreach (var msg in temp)
        {
            _queue.Enqueue(msg);
        }
    }
}

public class CommentMessage
{
    public string MatchId { get; set; } = string.Empty;
    public int PlayerId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
