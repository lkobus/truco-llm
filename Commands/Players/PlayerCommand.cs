using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using truco_net.Truco.Entities.Actions;
using truco_net.Truco.Models;
using truco_net.Truco.Entities.Players;

namespace truco_net.Commands.Players
{
    public class PlayerCommand : ICommand
    {
        private readonly Player _player;
        private readonly Match _match;
        private readonly string _matchId;
        private readonly TrucoNet.Infrastructure.CommentQueue? _commentQueue;
        
        public PlayerCommand(Player player, Match match, string matchId, TrucoNet.Infrastructure.CommentQueue? commentQueue = null)
        {
            _match = match;
            _player = player;
            _matchId = matchId;
            _commentQueue = commentQueue;
        }

        public async Task Execute(Mediator mediator)
        {            
            // Configura o TrucoService estático e matchId para players LLM/Gemini
            Player.TrucoServiceInstance = mediator.TrucoService;
            if (_player is LLMPlayer llmPlayer)
            {
                llmPlayer.CurrentMatchId = _matchId;
            }
            else if (_player is GeminiPlayer geminiPlayer)
            {
                geminiPlayer.CurrentMatchId = _matchId;
            }

            var actions = mediator.TrucoService.GetAvailableActions(_matchId, _player.Id, _match.Turn);
            
            var action = _player.Play(_match, actions);
            
            // Adiciona o comentário à lista se existir
            if (!string.IsNullOrWhiteSpace(action.Comment))
            {
                mediator.TrucoService.AddComment(_matchId, _player.Id, _player.Name, action.Comment, action.ActionType.ToString());
                
                // Envia para a fila de comentários
                _commentQueue?.Enqueue(_matchId, _player.Id, action.Comment, action.ActionType.ToString());
            }
            
            switch(action.ActionType)
            {
                case InGameActionsEnum.PlayCard:
                    mediator.TrucoService.SendCardToDesk(_match, action);
                    await mediator.EnqueueCommand(_matchId, new CloseMatchCommand(_matchId, _match));
                    return;                    
                case InGameActionsEnum.SkipTurn:
                    mediator.TrucoService.SendCardToDesk(_match, action);
                    await mediator.EnqueueCommand(_matchId, new CloseMatchCommand(_matchId, _match));                                        
                    return;
                case InGameActionsEnum.CallTruco:
                    mediator.TrucoService.RaiseBet(_matchId, _player.Id);
                    await mediator.EnqueueCommand(_matchId, new OnTrucoCommand(
                        _matchId,
                        _match,
                        _player,
                        _match.TeamA.Concat(_match.TeamB)
                            .FirstOrDefault(p => p.Id == _match.GetNextPlayerStartingFrom(_player.Id)),                        
                        _match.Reward,
                        mediator.CommentQueue
                    ));
                    return;                    
            }
            
            throw new NotImplementedException();
        }
    }
}