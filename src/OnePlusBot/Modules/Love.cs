using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class LoveCalcModule : ModuleBase<SocketCommandContext>
    {
        [Command("lovecalc")]
        [Summary("lovecalc search for Discord!")]
        public async Task LoveCalcAsync(string subjectA, [Remainder] string subjectB)
        {
            Random loveChance = new Random();
            int rand = loveChance.Next(0, 101);
            await ReplyAsync($":cupid: Love Chance between {subjectA} and {subjectB} is {rand}%.");
        }
    }
}
