using Microsoft.AspNetCore.Mvc;
using truco_net.Commands;
using Serilog;

namespace truco_net.Api;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly Mediator _mediator;
    private readonly TrucoNet.Infrastructure.CommentQueue _commentQueue;

    public MatchesController(Mediator mediator, TrucoNet.Infrastructure.CommentQueue commentQueue)
    {
        _mediator = mediator;
        _commentQueue = commentQueue;
    }

    /// <summary>
    /// Cria uma nova partida
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MatchId))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "MatchId é obrigatório"
            });
        }

        var created = await _mediator.CreateMatch(request.MatchId);

        if (!created)
        {
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = $"Partida {request.MatchId} já existe"
            });
        }

        // Enfileira comando para iniciar a partida
        // var startCommand = new StartMatchCommand
        // {
        //     MatchId = request.MatchId,
        //     Players = request.Players
        // };

        //await _mediator.EnqueueCommand(request.MatchId, startCommand);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = $"Partida {request.MatchId} criada com sucesso",
            Data = new { MatchId = request.MatchId, Players = request.Players }
        });
    }

    /// <summary>
    /// Lista todas as partidas ativas
    /// </summary>
    [HttpGet]
    public IActionResult GetMatches()
    {
        var matches = _mediator.GetActiveMatchIds();
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Partidas recuperadas com sucesso",
            Data = new 
            { 
                Count = _mediator.GetActiveMatchesCount(),
                Matches = matches 
            }
        });
    }

    /// <summary>
    /// Encerra uma partida
    /// </summary>
    [HttpDelete("{matchId}")]
    public async Task<IActionResult> EndMatch(string matchId)
    {
        var ended = await _mediator.EndMatch(matchId);

        if (!ended)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Partida {matchId} não encontrada"
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = $"Partida {matchId} encerrada com sucesso"
        });
    }

    /// <summary>
    /// Obtém o estado atual de uma partida
    /// </summary>
    [HttpGet("{matchId}/state")]
    public IActionResult GetMatchState(string matchId)
    {
        var state = _mediator.GetMatchState(matchId);

        if (state == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Partida {matchId} não encontrada"
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Estado da partida recuperado com sucesso",
            Data = state
        });
    }

    /// <summary>
    /// Obtém e remove comentários pendentes de uma partida
    /// </summary>
    [HttpGet("{matchId}/comments")]
    public IActionResult GetComments(string matchId)
    {
        var comments = _commentQueue.DequeueAll(matchId);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Comentários recuperados com sucesso",
            Data = comments
        });
    }
}
