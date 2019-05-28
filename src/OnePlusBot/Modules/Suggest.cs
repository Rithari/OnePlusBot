using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot._Extensions;

namespace OnePlusBot.Modules
{
    public class SuggestModule : ModuleBase<SocketCommandContext>
    {
        [Command("suggest")]
        [Summary("Suggests something to the server.")]
        public async Task SuggestAsync([Remainder] string title, string suggestion)
        {
            var channels = Context.Guild.TextChannels;
            var suggestionschannel = channels.FirstOrDefault(x => x.Name == "suggestions");

            var user = Context.Message.Author;
            var userpfp = Extensions.RealAvatarUrl(Context.Message.Author);

            if (suggestion.Contains("@everyone") || suggestion.Contains("@here")) suggestion = suggestion.Replace("" + suggestion, @"@(everyone|here)", "");

            var oldmessage = await suggestionschannel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithTitle(title).WithDescription(suggestion).WithAuthor(user, userpfp).WithFooter("Made by Rithari#0001", Extensions.RealAvatarUrl(Context.Client.CurrentUser)).WithCurrentTimestamp);

            await oldmessage.AddReactionsAsync(new Emoji[] { new Emoji(":OPYes:426070836269678614"), new Emoji(":OPNo:426072515094380555") });
            await Context.Message.DeleteAsync();




        }
    }
}
