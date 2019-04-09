using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
namespace OnePlusBot.Modules
{
    public class RestartModule : ModuleBase<SocketCommandContext>
    {
        [Command("restart")]
        [Summary("Restarts the bot.")]
        public async Task RestartAsync([Remainder] int mode)
        {
            var EmoteTrue = new Emoji(":success:499567039451758603");
            await Context.Message.AddReactionAsync(EmoteTrue);
            new Core().MainAsync(mode).GetAwaiter().GetResult();
        }
    }
}
