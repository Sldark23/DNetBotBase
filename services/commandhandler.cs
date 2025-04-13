using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;

    public CommandHandler(DiscordSocketClient client, IServiceProvider services)
    {
        _client = client;
        _services = services;
        _commands = new InteractionService(client);

        _client.InteractionCreated += HandleInteraction;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.Ready += RegisterCommands;
    }

    private async Task RegisterCommands()
    {
        await _commands.RegisterCommandsGloballyAsync();

        var commandList = _commands.SlashCommands;
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} Discord     {commandList.Count} Globally registered Slash Commands!");
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        
        if (!(interaction is SocketSlashCommand
            || interaction is SocketMessageComponent
            || interaction is SocketAutocompleteInteraction 
            || interaction is SocketMessageCommand))
        {
            return;
        }

        var botUser = (context.Guild as SocketGuild)?.GetUser(_client.CurrentUser.Id);
        var channel = context.Channel as ITextChannel;

        if (channel != null)
        {
            var botPermissions = botUser.GetPermissions(channel);

            if (!botPermissions.SendMessages || !botPermissions.EmbedLinks)
            {
                await context.Interaction.RespondAsync("🌳 - I do not have sufficient permissions to interact in this channel. I need **Send Messages** and **Embed Links** permissions.", ephemeral: true);
            }
        }

        await _commands.ExecuteCommandAsync(context, _services);
    }
}