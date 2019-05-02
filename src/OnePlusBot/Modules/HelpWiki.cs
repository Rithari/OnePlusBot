using OnePlusBot._Extensions;
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
            await Context.Channel.EmbedAsync(new EmbedBuilder().WithDescription("The command list was moved to: https://github.com/sapic/OnePlusBot/wiki/Command-List"));
        }


    }
}
