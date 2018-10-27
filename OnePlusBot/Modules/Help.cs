using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace OnePlusBot.Modules
{
   public class HelpModule : ModuleBase<SocketCommandContext>
    {

        [Command("help")]
        [Summary("Lists all available commands.")]
        public async Task HelpAsync()
        {
            await ReplyAsync("The command list was moved to: https://github.com/sapic/OnePlusBot/wiki/Command-List");
        }


    }
}
