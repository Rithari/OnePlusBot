using System.Threading.Tasks;
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
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            var channels = Context.Guild.TextChannels;
            var suggestionschannel = channels.FirstOrDefault(x => x.Name == "suggestions");

            var user = Context.Message.Author;

            if (suggestion.Contains("@everyone") || suggestion.Contains("@here"))
                return;

            var oldmessage = await suggestionschannel.EmbedAsync(new EmbedBuilder().WithDescription(suggestion + "\n **Suggested by**: " + user));

            var EmoteYes = new Emoji(":OPYes:426070836269678614");
            var EmoteNo = new Emoji(":OPNo:426072515094380555");
            await oldmessage.AddReactionAsync(EmoteYes);
            await oldmessage.AddReactionAsync(EmoteNo);
            await Context.Message.DeleteAsync();

           


         }
    }
}
