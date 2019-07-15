using System;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using OnePlusBot.Base;
namespace OnePlusBot.Modules
{
    public class ShutdownModule : ModuleBase<SocketCommandContext>
    {
        [Command("x")]
        [Summary("Emergency shutdown of the bot.")]
        [RequireOwner]
        public async Task ShutdownAsync()
        {
            await Context.Message.AddReactionAsync(Global.OnePlusEmote.SUCCESS);
            Environment.Exit(0);
        }

    }
}
