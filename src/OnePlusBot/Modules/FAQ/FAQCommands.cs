using System;
using System.Text;
using Discord.Commands;
using Discord;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;    
using System.Collections.ObjectModel;


namespace OnePlusBot.Modules.FAQ
{
    public partial class FAQ : InteractiveBase<SocketCommandContext>
    {

        [
            Command("faqCommands"),
            Summary("Lists the currently configured "),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public async Task ListFAQCommands()
        {
          Collection<Embed> embedsToPost = new Collection<Embed>();
          using(var db = new Database())
          {
            var currentEmbedBuilder = new EmbedBuilder();
            currentEmbedBuilder.WithTitle("Available faq commands");
            List<FAQCommand> commmands = db.FAQCommands.Include(c => c.CommandChannels)
            .ThenInclude(cc => cc.ChannelGroupReference)
            .ThenInclude(cgr => cgr.Channels)
            .OrderBy(c => c.ID)
            .ToList();
            
            var count = 0;
            foreach(var command in commmands)
            {
              count++;
              var channelMentions = getChannelsAsMentions(command.CommandChannels);
              currentEmbedBuilder.AddField(command.Name, channelMentions, true);
              if(((count % EmbedBuilder.MaxFieldCount) == 0) && command != commmands.Last())
              {
                embedsToPost.Add(currentEmbedBuilder.Build());
                currentEmbedBuilder = new EmbedBuilder();
                var currentPage = count / EmbedBuilder.MaxFieldCount + 1;
                currentEmbedBuilder.WithFooter(new EmbedFooterBuilder().WithText($"Page {currentPage}"));
              }  
            }
            embedsToPost.Add(currentEmbedBuilder.Build());
          }
          foreach (var embed in embedsToPost) {
            await Context.Channel.SendMessageAsync(embed: embed);
            await Task.Delay(200);
          }
        }

        private string getChannelsAsMentions(ICollection<FAQCommandChannel> channelGroups)
        {
          if(channelGroups == null) {
            return "no groups";
          }
          StringBuilder stringRepresentation = new StringBuilder();
          stringRepresentation.Append("Groups:");
          stringRepresentation.Append(string.Join(", ", channelGroups.Select(g => g.ChannelGroupReference.Name)));

          return stringRepresentation.ToString() != string.Empty ? stringRepresentation.ToString() : "no groups.";
        }
    }
}