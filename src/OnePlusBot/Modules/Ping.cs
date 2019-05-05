using System;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using OnePlusBot._Extensions;

namespace OnePlusBot.Modules
{
    public class PingModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Standard ping command.")]
        public async Task PingAsync()
        {
            var timestamp = Context.Message.Timestamp;
            var ping = DateTime.UtcNow - timestamp;

            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Pong....\nIn " + ping.Milliseconds + " ms"));
            await ReplyAsync("Ping...\nIn " + ping.Milliseconds + " ms");
        }
    }
}
