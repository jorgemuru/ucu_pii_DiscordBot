using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Ucu.Poo.DiscordBot.Services;

/// <summary>
/// Esta clase implementa el bot de Discord.
/// </summary>
public class Bot : IBot
{
    private ServiceProvider? serviceProvider;
    private readonly ILogger<Bot> _logger;
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;

    public Bot(ILogger<Bot> logger, IConfiguration configuration)
    {
        this._logger = logger;
        this._configuration = configuration;

        DiscordSocketConfig config = new()
        {
            AlwaysDownloadUsers = true,
            GatewayIntents =
                GatewayIntents.AllUnprivileged
                | GatewayIntents.MessageContent /*
                | GatewayIntents.GuildMembers*/
        };

        _client = new DiscordSocketClient(config);
    }

    public async Task StartAsync(ServiceProvider services)
    {
        string discordToken = _configuration["DiscordToken"] ?? throw new Exception("Falta el token");

        _logger.LogInformation("Iniciando el con token {Token}", discordToken);

        serviceProvider = services;

        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();

        _client.MessageReceived += HandleMessageAsync;
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Finalizando");
        await _client.LogoutAsync();
        await _client.StopAsync();
    }

    private async Task HandleMessageAsync(SocketMessage arg)
    {
        // Valida que el mensaje sea del usuario
        if (arg is not SocketUserMessage message || message.Author.IsBot)
        {
            return;
        }

        int position = 0;
        char commandPrefix = '/';
        bool messageIsCommand = message.HasCharPrefix(commandPrefix, ref position);

        // Valida que el mensaje sea un comando
        if (!messageIsCommand)
        {
            return;
        }

        // Convierte el mensaje en minusculas y le saca el prefijo
        string command = message.Content[position..].ToLower().Trim();
        string response;

        // Maneja los comandos que se envian al bot
        switch (command)
        {
            case "hola":
                response = $"Hola {message.Author} ! Cómo estás?";
                break;
            default:
                response = "No conozco ese comando !";
                break;
        }

        // Envia la respuesta
        await message.Channel.SendMessageAsync(response);
    }
}