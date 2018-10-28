using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;

namespace OnePlusBot.Modules
{
    public class MagicBallModule : ModuleBase<SocketCommandContext>
    {
        [Command("8ball")]
        [Summary("Magic 8Ball for Discord!")]
        public async Task MagicBallAsync([Remainder] string search)
        {
            try
            {
                string answer = new[]
                    {
                        "It is certain.", "It is decidedly so.", "Without a doubt.", "Yes, definitely.", "You may rely on it.", "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.", "Reply hazy to try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.", "Don't count on it.", "My reply is no.", "Outlook not so good.", "Very doubtful.",
                        "My sources say no."
                    }[new Random().Next(0, 20)];

                await ReplyAsync(answer);
            }
            catch(Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}
