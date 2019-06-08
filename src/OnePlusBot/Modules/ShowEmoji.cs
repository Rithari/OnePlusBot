using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class ShowEmoji : ModuleBase<SocketCommandContext>
    {
        [Command("showemotes")]
        [Alias("se")]
        [Summary("Shows avatar of a user.")]
        public async Task Showemojis([Remainder] string _) // need to have the parameter so that the message.tags gets populated
        {
            var tags = Context.Message.Tags.Where(t => t.Type == TagType.Emoji).Select(t => (Emote)t.Value);
            
            var result = string.Join("\n", tags.Select(m => "**Name:** " + m + " **Link:** " + m.Url));

            if (string.IsNullOrWhiteSpace(result))
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("No special emojis found."));
            else
                await Context.Channel.SendMessageAsync(result).ConfigureAwait(false);
        }
    }
}
