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
            var pageCount = 1;
            foreach(var command in commmands)
            {
              count++;
              var channelMentions = getChannelsAsMentions(command.CommandChannels);
              foreach(string channelMention in channelMentions) {
              if(((count % EmbedBuilder.MaxFieldCount) == 0) && command != commmands.Last() || 
                ((currentEmbedBuilder.Length + channelMention.Length + command.Name.Length) > EmbedBuilder.MaxEmbedLength))
                {
                  embedsToPost.Add(currentEmbedBuilder.Build());
                  currentEmbedBuilder = new EmbedBuilder();
                  pageCount += 1;
                  currentEmbedBuilder.WithFooter(new EmbedFooterBuilder().WithText($"Page {pageCount}"));
                }
                currentEmbedBuilder.AddField(command.Name, channelMention, true);
              }

            }
            embedsToPost.Add(currentEmbedBuilder.Build());
          }
          foreach (var embed in embedsToPost) {
            await Context.Channel.SendMessageAsync(embed: embed);
            await Task.Delay(200);
          }
        }

        private List<string> getChannelsAsMentions(ICollection<FAQCommandChannel> channelGroups)
        {
          if(channelGroups == null) {
            return new List<string>()
            {
              "no groups",
            };
          }
          var groups = new List<string>();
          foreach(FAQCommandChannel fch in channelGroups)
          {
            extendIfNecessary(groups, $"Group: {fch.ChannelGroupReference.Name} {Environment.NewLine}Channels: ");
            foreach(ChannelInGroup ch in fch.ChannelGroupReference.Channels) {
              extendIfNecessary(groups, $"<#{ch.ChannelId}> ");
            }
            if(fch.ChannelGroupReference.Channels.Count == 0) {
              extendIfNecessary(groups, " no channels.");
            }
            extendIfNecessary(groups, Environment.NewLine);
          }
          var defaultList = new List<string>()
          {
            "no groups",
          };
          return groups.Count() != 0 ? groups : defaultList;
        }

        private void extendIfNecessary(List<string> list, string toAdd)
        {
          if(list.Count() == 0 || (list.Last().Length + toAdd.Length) > 1024)
          {
            list.Add(toAdd);
          }
          else
          {
            list[list.Count() - 1] = list[list.Count() - 1] + toAdd;
          }
        }
    }
}