using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class SkidModule : ModuleBase<SocketCommandContext>
    {
        [Command("skid")]
        [Summary("A skid level mesasurement.")]
        public async Task SkidAsync([Remainder] string user)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 100);

            await ReplyAsync(user + "'s skid level is: " + randomNumber + "%");
        }
    }
}
