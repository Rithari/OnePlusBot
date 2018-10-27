﻿using System;
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


            var oldmessage = await suggestionschannel.SendMessageAsync(suggestion + "\n **Suggested by**: " + user.Mention);
            var EmoteYes = new Emoji(":OPYes:426070836269678614");
            var EmoteNo = new Emoji(":OPNo:426072515094380555");
            await oldmessage.AddReactionAsync(EmoteYes);
            await oldmessage.AddReactionAsync(EmoteNo);
            await Context.Message.DeleteAsync();

           


         }
    }
}
