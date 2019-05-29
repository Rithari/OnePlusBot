using System;
using System.Threading.Tasks;
using Discord.Commands;
using OnePlusBot.Base;

namespace OnePlusBot.Modules
{
    public class RouletteModule : ModuleBase<SocketCommandContext>
    {
        [Command("roulette")]
        [Summary("Russian roulette for Discord!")]
        public async Task RouletteAsync()
        {
            var answers = new[]
            {
                ":gun: *click*, no bullet in there for you this round.\r\n",
                ":gun: :boom:, you died! :skull:\r\n", ":gun: *click*, no bullet in there for you this round.\r\n"
            };

            var answer = answers[Global.Random.Next(3) % 2]; // 1 out of 3 possibility of death
            await ReplyAsync(answer);
        }
    }
}
