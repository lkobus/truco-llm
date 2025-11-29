using truco_net.Truco.Entities.Players;
using truco_net.Truco.Models;

namespace truco_net.Examples;

/// <summary>
/// Exemplo de como usar o LLMPlayer com a API da OpenAI
/// </summary>
public class LLMPlayerExample
{
    public static void CreateLLMPlayer()
    {
        // Substitua "YOUR_API_KEY_HERE" pela sua chave da OpenAI
        string apiKey = "YOUR_API_KEY_HERE";
        
        // Cria um jogador que usa IA para tomar decisões
        var llmPlayer = new LLMPlayer(id: 1, name: "AI Player", apiKey: apiKey);
        
        // O LLMPlayer agora pode ser usado em uma partida normalmente
        // Ele irá consultar a OpenAI GPT-4o-mini para tomar decisões estratégicas
    }
    
    public static List<Player> CreateMixedTeam(string apiKey)
    {
        // Exemplo: Time com jogador IA e jogador random
        var team = new List<Player>
        {
            new LLMPlayer(1, "AI Strategic", apiKey),
            new RandomCardPlayer(2, "Random Joe")
        };
        
        return team;
    }
}
