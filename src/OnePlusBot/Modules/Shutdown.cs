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
            var successEmote = Emote.Parse("<:success:499567039451758603>");

            await Context.Message.AddReactionAsync(successEmote);
            Environment.Exit(0);
        }

    }
}
