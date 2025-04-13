using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;

public class Program
{
    private static DiscordSocketClient _client;
    private static CommandHandler _commandHandler;

    public static async Task Main() => await RunBotAsync();

    public static async Task RunBotAsync()
    {
        var _config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
            MessageCacheSize = 100,
            AlwaysDownloadUsers = true
        };
        _client = new DiscordSocketClient(_config);

        var services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton<InteractionService>()
            .BuildServiceProvider();
        
        _commandHandler = new CommandHandler(_client, services);

        _client.Log += Log;
        _client.Disconnected += HandleDisconnect;
        _client.Ready += Ready;

        Env.Load();

        string token = Environment.GetEnvironmentVariable("token");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Error: Bot token not found!");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        await _commandHandler.InitializeAsync();

        await SetCustomStatus();
        StartStatusChangeTimer();
        
        await Task.Delay(-1);
    }
    
    private static Task HandleDisconnect(Exception ex)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} Discord     Bot disconnected: {ex.Message}. Trying to reconnect...");
        
        Task.Run(async () =>
        {
            int attempts = 0;
            while (_client.ConnectionState != ConnectionState.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine($"Attempt {attempts} to reconnect...");
                    await Task.Delay(5000);
                    
                    if (_client.ConnectionState == ConnectionState.Connected)
                    {
                        Console.WriteLine("Bot already reconnected, aborting attempt.");
                        break;
                    }
                    
                    if (_client.LoginState == LoginState.LoggedIn)
                    {
                        await _client.StartAsync();
                    }
                    else
                    {
                        await _client.LogoutAsync();
                        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token"));
                        await _client.StartAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error trying to reconnect: {e.Message}");
                }
                
                if (attempts >= 10)
                {
                    Console.WriteLine("Failed to reconnect after 10 attempts. Aborting...");
                    break;
                }
            }
        });
        return Task.CompletedTask;
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private static Task Ready()
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} Discord     The bot [{_client.CurrentUser.Username}] is online!");
        return Task.CompletedTask;
    }

    private static async Task SetCustomStatus()
    {
        if (_client.ConnectionState != ConnectionState.Connected) return;
        
        var statusMessages = new[]
        {
            "🫦 - Base made by Coelhinho (872215189406224404)"
        };

        var random = new Random();
        var statusIndex = random.Next(statusMessages.Length);

        var activity = new CustomStatusGame(statusMessages[statusIndex]);

        await _client.SetActivityAsync(activity);
    }

    private static void StartStatusChangeTimer()
    {
        var timer = new System.Timers.Timer(15000);
        timer.Elapsed += async (sender, e) => await SetCustomStatus();
        timer.Start();
    }
}