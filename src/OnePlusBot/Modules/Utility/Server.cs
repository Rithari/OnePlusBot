using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot.Base;
using OnePlusBot.Helpers;
using System.Runtime.InteropServices;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using System;
using Discord.WebSocket;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;

namespace OnePlusBot.Modules.Utility
{

    public partial class Utility : ModuleBase<SocketCommandContext>
    {

        [
            Command("suggest"),
            Summary("Suggests something to the server."),
            CommandDisabledCheck
        ]
        public async Task SuggestAsync([Remainder] string suggestion)
        {
            var suggestionsChannel = Context.Guild.GetTextChannel(Global.PostTargets[PostTarget.SUGGESTIONS]);
            var user = Context.Message.Author;

            if (suggestion.Contains("@everyone") || suggestion.Contains("@here"))
                return;

            var oldmessage = await suggestionsChannel.EmbedAsync(new EmbedBuilder()
                .WithColor(9896005)
                .WithDescription(suggestion)
                .WithFooter(user.ToString()));

            await oldmessage.AddReactionsAsync(new IEmote[]
            {
              StoredEmote.GetEmote(Global.OnePlusEmote.OP_NO), 
              StoredEmote.GetEmote(Global.OnePlusEmote.OP_YES)
            });
            
            await Context.Message.DeleteAsync();
        }

        [
            Command("news"),
            Summary("Posts a News article to the server."),
            RequireRole("journalist"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> NewsAsync([Remainder] string news)
        {
          var guild = Context.Guild;

          var user = (SocketGuildUser)Context.Message.Author;
          
          var newsChannel = guild.GetTextChannel(Global.PostTargets[PostTarget.NEWS]) as SocketTextChannel;
          var newsRole = guild.GetRole(Global.Roles["news"]);

          if (news.Contains("@everyone") || news.Contains("@here") || news.Contains("@news")) {
            return CustomResult.FromError("Your news article contained one or more illegal pings!");
          }

          IMessage posted;
          var messageToPost = newsRole.Mention + Environment.NewLine + "- " + Context.Message.Author;
          var builder = new EmbedBuilder().WithDescription(news);
          if( Context.Message.Attachments.Any())
          {
            var attachment = Context.Message.Attachments.First();
            builder.WithImageUrl(attachment.ProxyUrl).Build();
          }
          posted = await newsChannel.SendMessageAsync(messageToPost, embed: builder.Build());
              
          Global.NewsPosts[Context.Message.Id] = posted.Id;

          return CustomResult.FromSuccess();
        }


        /// <summary>
        /// Shows general information about the server: name, owner, creation date, channel count, features, members and custom emoji
        /// </summary>
        /// <param name="guildName"></param>
        /// <returns></returns>
        [
            Command("serverinfo"),
            Summary("Shows server information."),
            CommandDisabledCheck
        ]
        public async Task sinfo(string guildName = null)
        {
            var channel = (ITextChannel)Context.Channel;
            guildName = guildName?.ToUpperInvariant();
            SocketGuild guild;
            if (string.IsNullOrWhiteSpace(guildName))
                guild = (SocketGuild)channel.Guild;
            else
                guild = Global.Bot.Guilds.FirstOrDefault(g => g.Name.ToUpperInvariant() == guildName.ToUpperInvariant());
            if (guild == null)
                return;
            var ownername = guild.GetUser(guild.OwnerId);
            var textchn = guild.TextChannels.Count();
            var voicechn = guild.VoiceChannels.Count();

            var createdAt = new DateTime(2015, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(guild.Id >> 22);
            var features = string.Join("\n", guild.Features);
            if (string.IsNullOrWhiteSpace(features))
                features = "-";
            var embed = new EmbedBuilder()
                .WithAuthor("Server info")
                .WithTitle(guild.Name)
                .AddField(fb => fb.WithName("ID").WithValue(guild.Id.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Owner").WithValue(ownername.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Members").WithValue(guild.MemberCount.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Text channels").WithValue(textchn.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Voice channels").WithValue(voicechn.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Created at").WithValue($"{ Extensions.FormatDateTime(createdAt)}").WithIsInline(true))
                .AddField(fb => fb.WithName("Region").WithValue(guild.VoiceRegionId.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Roles").WithValue((guild.Roles.Count - 1).ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Features").WithValue(features).WithIsInline(true))
                .WithColor(9896005);
            if (Uri.IsWellFormedUriString(guild.IconUrl, UriKind.Absolute))
                embed.WithThumbnailUrl(guild.IconUrl);
            if (guild.Emotes.Any())
            {
                 
                var strings = new Collection<StringBuilder>();
                var currentStringBuilder = new StringBuilder();
                strings.Add(currentStringBuilder);
                foreach(var emote in guild.Emotes)
                {
                    var emoteText = emote.ToString();
                    // doesnt seem to have a constant for that, exception message indicated the max length is 1024
                    if((currentStringBuilder.ToString() + emoteText).Length > 1024)
                    {
                        currentStringBuilder = new StringBuilder();
                        currentStringBuilder.Append(emoteText);
                        strings.Add(currentStringBuilder);
                    }
                    else 
                    {
                        currentStringBuilder.Append(emoteText);
                    }
                }
                var counter = 1;
                foreach(var emoteText  in strings)
                {
                    var headerText = counter > 1 ? "#" + counter: "";
                    var headerPostText = counter == 1 ? $"(Total: {guild.Emotes.Count})" : "";
                    counter++;
                    embed.AddField(fb =>
                    fb.WithName($"Custom emojis {headerText} {headerPostText}")
                    .WithValue(emoteText.ToString()));
                }
              
            }
            await Context.Channel.EmbedAsync(embed).ConfigureAwait(false);
        }

          /// <summary>
        /// Command used to show the currently availble commands for the current channel, or for the given channel
        /// </summary>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("availableCommands"),
            Summary("Shows the available commands for the current (or given channel) channel")
        ]
        public async Task<RuntimeResult> ShowAvailableCommands([Optional] SocketGuildChannel targetChannel)
        {
          ISocketMessageChannel channelToExecuteFor = targetChannel == null ?  Context.Channel : targetChannel as ISocketMessageChannel;
          var embedBuilder = new EmbedBuilder();
          embedBuilder.WithTitle($"Currently available commands for channel {channelToExecuteFor.Name}");
          StringBuilder sb = new StringBuilder();
          using(var db = new Database())
          {
            var modules = db.Modules;
            foreach(var module in modules){
              var commandsInModule = db.Commands.Include(c => c.GroupsCommandIsIn)
              .ThenInclude(grp => grp.ChannelGroupReference)
              .ThenInclude(grp => grp.Channels)
              .Where(coChGrp => coChGrp.ModuleId == module.ID);
              sb.Append($"\n Module: {module.Name} \n");
              if(commandsInModule.Any())
              {
                foreach(var command in commandsInModule)
                {
                  if(command.CommandEnabled(channelToExecuteFor.Id) || command.GroupsCommandIsIn.Count() == 0)
                    sb.Append($"`{command.Name}` ");
                  }
                }
              }
            }
              
          embedBuilder.WithDescription(sb.ToString());
          await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
          await Task.Delay(200);
          return CustomResult.FromSuccess();
        }


    }
}
