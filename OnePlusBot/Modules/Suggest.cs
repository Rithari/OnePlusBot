using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;

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

            await suggestionschannel.SendMessageAsync(suggestion + "\n **Suggested by**: " + user.Mention);

            await Context.Message.DeleteAsync();

           


         }
    }
}
