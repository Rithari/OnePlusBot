using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using OnePlusBot._Extensions;

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

                await Context.Channel.EmbedAsync(new EmbedBuilder()
                    .WithColor(9896005)
                    .AddField(efb => efb.WithName("🎱 The 8 Ball Says:").WithValue(answer).WithIsInline(false)));
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}
