using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using OnePlusBot.Base;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class MagicBallModule : ModuleBase<SocketCommandContext>
    {
        [Command("8ball")]
        [Summary("Magic 8Ball for Discord!")]
        public async Task MagicBallAsync([Remainder] string search)
        {
            var answers = GetAnswers();
            var answer = answers[Global.Random.Next(answers.Length)];

            await Context.Channel.EmbedAsync(new EmbedBuilder()
                .WithColor(9896005)
                .AddField(efb =>
                {
                    efb.Name = "🎱 The 8 Ball Says:";
                    efb.Value = answer;
                }));
        }
        
        private static string[] GetAnswers()
        {
            return new[]
            {
                "It is certain.", "It is decidedly so.", "Without a doubt.", "Yes, definitely.", "You may rely on it.",
                "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.",
                "Reply hazy to try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.",
                "Concentrate and ask again.", "Don't count on it.", "My reply is no.", "Outlook not so good.",
                "Very doubtful.",
                "My sources say no."
            };
        }
    }
}
