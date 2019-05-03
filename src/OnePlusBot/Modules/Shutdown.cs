using System;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
namespace OnePlusBot.Modules
{
    public class ShutdownModule : ModuleBase<SocketCommandContext>
    {
        [Command("x")]
        [Summary("Emergency shutdown of the bot.")]
        [RequireOwner]
        public async Task ShutdownAsync()
        {
            var EmoteTrue = new Emoji(":success:499567039451758603");
            await Context.Message.AddReactionAsync(EmoteTrue);
            Environment.Exit(0);
        }

    }
}
