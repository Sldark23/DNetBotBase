using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

public class PingCommandModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand(name: "ping", description: "📡 - Shows the bot's latency.")]
    public async Task Ping()
    {
        int apiLatency = Context.Client.Latency;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await RespondAsync("Measuring latency...");
        stopwatch.Stop();

        int messageLatency = (int)stopwatch.ElapsedMilliseconds;

        await Task.Delay(3000);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Content = $"📡 Pong!\n" +
                          $"**API Latency:** {apiLatency}ms\n" +
                          $"**Message Latency:** {messageLatency}ms";
        });
    }
}