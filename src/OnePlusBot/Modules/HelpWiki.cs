using OnePlusBot._Extensions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System;

namespace OnePlusBot.Modules
{
   public class HelpModule : ModuleBase<SocketCommandContext>
    {

        [Command("help")]
        [Summary("Lists all available commands.")]
  
        public async Task HelpAsync()
        {
            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("The command list was moved to: https://github.com/Rithari/OnePlusBot/wiki/Command-List"));
        }


    }
}
