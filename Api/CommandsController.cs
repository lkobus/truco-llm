using Microsoft.AspNetCore.Mvc;
using truco_net.Commands;
using truco_net.Truco.Models;
using truco_net.Truco.Entities.Players;
using System.Text.Json;

namespace truco_net.Api;

[ApiController]
[Route("api/[controller]")]
public class CommandsController : ControllerBase
{
    private readonly Mediator _mediator;

    public CommandsController(Mediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Inicia uma nova partida de truco
    /// </summary>
    [HttpPost("start-match")]
    public async Task<IActionResult> StartMatch([FromBody] StartMatchRequest request)
    {
        // Gera MatchId automaticamente se não fornecido
        var matchId = string.IsNullOrWhiteSpace(request.MatchId) 
            ? $"match-{Guid.NewGuid().ToString()[..8]}" 
            : request.MatchId;

        if (request.TeamA == null || request.TeamA.Count != 2)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "TeamA deve conter exatamente 2 jogadores"
            });
        }

        if (request.TeamB == null || request.TeamB.Count != 2)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "TeamB deve conter exatamente 2 jogadores"
            });
        }

        // Verifica se a partida já existe
        if (_mediator.TrucoService.Matches.ContainsKey(matchId))
        {
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = $"Partida {matchId} já existe"
            });
        }

        try
        {
            // Cria a fila de comandos para a partida
            var queueCreated = await _mediator.CreateMatch(matchId);
            if (!queueCreated)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erro ao criar fila de comandos para a partida"
                });
            }

            // Criar jogadores do time A
            var teamA = request.TeamA.Select(p => CreatePlayer(p)).ToList();
            
            // Criar jogadores do time B
            var teamB = request.TeamB.Select(p => CreatePlayer(p)).ToList();

            // Criar e executar comando
            var command = new StartMatchCommand(
                matchId,
                teamA,
                teamB,
                request.StartRoundPlayer
            );

            await command.Execute(_mediator);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Partida iniciada com sucesso",
                Data = new
                {
                    MatchId = matchId,
                    TeamA = request.TeamA,
                    TeamB = request.TeamB,
                    StartRoundPlayer = request.StartRoundPlayer
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Erro ao iniciar partida: {ex.Message}"
            });
        }
    }

    private Player CreatePlayer(PlayerDto dto)
    {
        return dto.Type.ToLower() switch
        {
            "randomcardplayer" => new RandomCardPlayer(dto.Id, dto.Name),
            "llmplayer" => CreateLLMPlayer(dto),
            "geminiplayer" => CreateGeminiPlayer(dto),
            _ => new RandomCardPlayer(dto.Id, dto.Name) // Default
        };
    }

    private Player CreateLLMPlayer(PlayerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ApiKey))
        {
            throw new ArgumentException($"ApiKey é obrigatória para o jogador {dto.Name} do tipo LLMPlayer");
        }

        return new LLMPlayer(dto.Id, dto.Name, dto.ApiKey);
    }

    private Player CreateGeminiPlayer(PlayerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ApiKey))
        {
            throw new ArgumentException($"ApiKey é obrigatória para o jogador {dto.Name} do tipo GeminiPlayer");
        }

        return new GeminiPlayer(dto.Id, dto.Name, dto.ApiKey);
    }

    /// <summary>
    /// Envia um comando para a fila de uma partida
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendCommand([FromBody] CommandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MatchId))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "MatchId é obrigatório"
            });
        }

        // ICommand? command = request.CommandType.ToLower() switch
        // {
        //     "playcard" => new PlayCardCommand
        //     {
        //         MatchId = request.MatchId,
        //         PlayerId = request.Parameters.GetValueOrDefault("playerId")?.ToString() ?? "",
        //         Card = request.Parameters.GetValueOrDefault("card")?.ToString() ?? ""
        //     },
        //     "calltruco" => new CallTrucoCommand
        //     {
        //         MatchId = request.MatchId,
        //         PlayerId = request.Parameters.GetValueOrDefault("playerId")?.ToString() ?? ""
        //     },
        //     _ => null
        // };

        // if (command == null)
        // {
        //     return BadRequest(new ApiResponse<object>
        //     {
        //         Success = false,
        //         Message = $"Tipo de comando '{request.CommandType}' não reconhecido"
        //     });
        // }

        //var enqueued = await _mediator.EnqueueCommand(request.MatchId, command);

        // if (!enqueued)
        // {
        //     return NotFound(new ApiResponse<object>
        //     {
        //         Success = false,
        //         Message = $"Partida {request.MatchId} não encontrada"
        //     });
        // }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Comando enfileirado com sucesso",
            Data = new 
            { 
                CommandType = request.CommandType,
                MatchId = request.MatchId
            }
        });
    }
}
