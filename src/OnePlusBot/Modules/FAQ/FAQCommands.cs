using Discord;
using Discord.Commands;
using OnePlusBot.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using Microsoft.EntityFrameworkCore;


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
          foreach(FAQCommandChannel fch in channelGroups)
          {
            stringRepresentation.Append($"Group: {fch.ChannelGroupReference.Name} {Environment.NewLine}Channels: ");
            foreach(ChannelInGroup ch in fch.ChannelGroupReference.Channels) {
              stringRepresentation.Append($"<#{ch.ChannelId}> ");
            }
            if(fch.ChannelGroupReference.Channels.Count == 0) {
              stringRepresentation.Append(" no channels.");
            }
            stringRepresentation.Append(Environment.NewLine);
          }

          return stringRepresentation.ToString() != string.Empty ? stringRepresentation.ToString() : "no groups.";
        }
    }
}