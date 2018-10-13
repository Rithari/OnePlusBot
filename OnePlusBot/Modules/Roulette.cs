using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Text.RegularExpressions;

namespace OnePlusBot.Modules
{
    public class RouletteModule : ModuleBase<SocketCommandContext>
    {
        [Command("roulette")]
        [Summary("Russian roulette for Discord!")]
        public async Task RouletteAsync()
        {
            await ReplyAsync(new[] { ":gun: :boom:, you died! :skull:\r\n", ":gun: *click*, no bullet in there for you this round.\r\n", ":gun: *click*, no bullet in there for you this round.\r\n", ":gun: *click*, no bullet in there for you this round.\r\n" }[new Random().Next(0, 4)]);
        }
    }
}
