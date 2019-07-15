﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot.Base;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class SuggestModule : ModuleBase<SocketCommandContext>
    {
        [Command("suggest")]
        [Summary("Suggests something to the server.")]
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            var suggestionsChannel = Context.Guild.GetTextChannel(Global.Channels["suggestions"]);
            var user = Context.Message.Author;

            if (suggestion.Contains("@everyone") || suggestion.Contains("@here"))
                return;

            var oldmessage = await suggestionsChannel.EmbedAsync(new EmbedBuilder()
                .WithColor(9896005)
                .WithDescription(suggestion)
                .WithFooter(user.ToString()));

            await oldmessage.AddReactionsAsync(new IEmote[]
            {
                Global.OnePlusEmote.OP_YES, 
                Global.OnePlusEmote.OP_NO
            });
            
            await Context.Message.DeleteAsync();
        }
    }
}
