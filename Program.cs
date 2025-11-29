using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using truco_net;
using truco_net.Truco;
using truco_net.Truco.Models;

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/truco-net-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
        rollOnFileSizeLimit: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("=== Iniciando Truco.NET Server ===");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog
    builder.Host.UseSerilog();

    // Adicionar serviços
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() 
        { 
            Title = "Truco.NET API", 
            Version = "v1",
            Description = "API para gerenciar partidas de truco com sistema de filas por partida"
        });
    });

    // Adicionar CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Registrar o Mediator como Singleton
    var trucoService = new TrucoService();
    var commentQueue = new TrucoNet.Infrastructure.CommentQueue();
    builder.Services.AddSingleton(trucoService);
    builder.Services.AddSingleton(commentQueue);
    builder.Services.AddSingleton(new Mediator(trucoService, commentQueue));    



    
    // var deck = new TrucoDeck();
    // var team_a = new List<Player>
    // {
    //     new truco_net.Truco.Entities.Players.RandomCardPlayer(1, "Jogador 1"),
    //     new truco_net.Truco.Entities.Players.RandomCardPlayer(2, "Jogador 2"),
        
    // };
    // var team_b = new List<Player>
    // {   
    //     new truco_net.Truco.Entities.Players.RandomCardPlayer(3, "Jogador 3"),
    //     new truco_net.Truco.Entities.Players.RandomCardPlayer(4, "Jogador 4"),
    // };
    // var startRoundPlayer = 1;
    // trucoService.StartMatch("match test", team_a, team_b, startRoundPlayer);
    // while(!trucoService.IsOver)
    // {        
    //     while(!trucoService.IsMatchFinished)
    //     {
    //         var round = 1;
    //         for(var j = 0; j < 4; j++)
    //         {
    //             var currentPlayer = trucoService.GetCurrentPlayerId("match test");
    //             var actions = trucoService.GetAvailableActions("match test", currentPlayer, round);
    //             var player = team_a.Concat(team_b).FirstOrDefault(p => p.Id == currentPlayer);
    //             var action = player.Play(trucoService.Matches["match test"], actions);
    //             trucoService.SendCardToDesk(trucoService.Matches["match test"], action);
    //         }
    //         trucoService.CloseMatch(trucoService.Matches["match test"], "match test");
    //         round++;
    //     }
        
    //     trucoService.StartMatch("match test", team_a, team_b, trucoService.Matches["match test"].GetNextPlayerStartingFrom(startRoundPlayer));
    // }
        
    
    // Configurar URL
    builder.WebHost.UseUrls("http://0.0.0.0:5002");

    var app = builder.Build();

    // Configurar o pipeline HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Truco.NET API v1");
            c.RoutePrefix = string.Empty; // Swagger na raiz
        });
    }

    app.UseCors("AllowAll");
    
    // Servir arquivos estáticos da pasta assets
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "assets")),
        RequestPath = "/assets"
    });
    
    // Servir arquivos estáticos do Site
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "Site")),
        RequestPath = ""
    });

    app.UseAuthorization();
    app.MapControllers();

    // Endpoint de health check
    app.MapGet("/health", (Mediator mediator) =>
    {
        return Results.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            ActiveMatches = mediator.GetActiveMatchesCount()
        });
    });

    Log.Information("Servidor configurado. Aguardando requisições em http://localhost:5000");
    Log.Information("Swagger UI disponível em http://localhost:5000");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao iniciar a aplicação");
}
finally
{
    Log.Information("=== Encerrando Truco.NET Server ===");
    Log.CloseAndFlush();
}

