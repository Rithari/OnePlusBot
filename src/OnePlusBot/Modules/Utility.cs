using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot.Base;
using OnePlusBot.Helpers;
using System.Globalization;
using System.Runtime.InteropServices;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using System;
using Discord.WebSocket;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;

namespace OnePlusBot.Modules
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        [
            Command("showavatar"),
            Summary("Shows avatar of a user."),
            CommandDisabledCheck
        ]
        public async Task Avatar(IGuildUser user = null)
        {
            if (user == null)
                user = (IGuildUser) Context.User;

            var uri = user.RealAvatarUrl(4096).ToString();

            if (uri == null)
            {
                await Context.Channel.EmbedAsync(new EmbedBuilder()
                    .WithColor(9896005)
                    .WithDescription("User has no avatar."));
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithColor(9896005);
            embed.Url = uri;

            embed.AddField(x =>
            {
                x.Name = "Username";
                x.Value = Extensions.FormatMentionDetailed(user);
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Image";
                x.Value = $"[Link]({uri})";
                x.IsInline = true;
            });

            embed.ImageUrl = uri;
            
            await Context.Channel.EmbedAsync(embed);
        }


        /// <summary>
        /// Shows information about the given (or executing in case given is null) user. This information includes registration/join date, current status and nickname.
        /// </summary>
        /// <param name="user">The <see cref="Discord.IGuildUser"> to post the informatoin for</param>
        /// <returns>Task</returns>
        [
            Command("userinfo"),
            Summary("Displays User Information"),
            CommandDisabledCheck
        ]
        public async Task UserInfo([Optional] IGuildUser user)
        {
          user = user ?? (IGuildUser) Context.User;
          
          var embedBuilder = new EmbedBuilder();

          embedBuilder.WithColor(9896005);
          embedBuilder.WithAuthor(x =>
          {
              x.Name = user.Username;
          });

          embedBuilder.ThumbnailUrl = user.GetAvatarUrl();

          embedBuilder.AddField("Status", user.Status.ToString(), true);

          if(user.Nickname != null)
          {
            embedBuilder.AddField("Nickname", user.Nickname, true);
          }
          
          embedBuilder.AddField("Activity", user.Activity?.Name ?? "Nothing", true);

          if (user.JoinedAt.HasValue)
          {
            embedBuilder.AddField("Joined", Extensions.FormatDateTime(user.JoinedAt.Value.DateTime), true);
          }

          embedBuilder.AddField("Registered", Extensions.FormatDateTime(user.CreatedAt.DateTime), true);
                      
          await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }


        /// <summary>
        /// Searches for all of the custom emoji in the parameters and posts a bigger image and the direct link
        /// </summary>
        /// <param name="_">Text containing the emoji with the URLs </param>
        /// <returns>Task</returns>
        [
            Command("se"),
            Summary("Shows bigger version of am emote."),
            CommandDisabledCheck
        ]
        public async Task Showemojis([Remainder] string _) // need to have the parameter so that the message.tags gets populated
        {
            var tags = Context.Message.Tags.Where(t => t.Type == TagType.Emoji).Select(t => (Emote)t.Value);
            
            var result = string.Join("\n", tags.Select(m => "**Name:** " + m + " **Link:** " + m.Url));

            if (string.IsNullOrWhiteSpace(result)) 
            {
               await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("No special emojis found."));
            }
            else 
            {
              await Context.Channel.SendMessageAsync(result);
            }
        }

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
              Global.Emotes[Global.OnePlusEmote.OP_NO].GetAsEmote(), 
              Global.Emotes[Global.OnePlusEmote.OP_YES].GetAsEmote()
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
            
            var needsAttachments = Context.Message.Attachments.Count() > 0;
            
            var newsChannel = guild.GetTextChannel(Global.PostTargets[PostTarget.NEWS]) as SocketNewsChannel;
            var newsRole = guild.GetRole(Global.Roles["news"]);

            if (news.Contains("@everyone") || news.Contains("@here") || news.Contains("@news")) 
                return CustomResult.FromError("Your news article contained one or more illegal pings!");

            await newsRole.ModifyAsync(x => x.Mentionable = true);
            IMessage posted;
            var messageToPost = news + Environment.NewLine + Environment.NewLine + newsRole.Mention + Environment.NewLine + "- " + Context.Message.Author;
            try {
                if(needsAttachments)
                {
                    var attachment = Context.Message.Attachments.First();
                    WebClient client = new WebClient();
                    client.DownloadFile(attachment.Url, attachment.Filename);
                    posted = await newsChannel.SendFileAsync(attachment.Filename, messageToPost);
                    File.Delete(attachment.Filename);  
                }
                else
                {
                    posted = await newsChannel.SendMessageAsync(messageToPost);
                }
            }
            finally 
            {
                await newsRole.ModifyAsync(x => x.Mentionable = false);
            }
            
           

            Global.NewsPosts[Context.Message.Id] = posted.Id;

            return CustomResult.FromSuccess();
        }

         public readonly DiscordSocketClient _client;


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
                guild = _client.Guilds.FirstOrDefault(g => g.Name.ToUpperInvariant() == guildName.ToUpperInvariant());
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

        [
            Command("echo"),
            Summary("Echoes back the remainder argument of the command."),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public Task EchoAsync([Remainder] string text)
        {
            return ReplyAsync(text);
        }

        [
            Command("ping"),
            Summary("Standard ping command."),
            CommandDisabledCheck
        ]
        public async Task PingAsync()
        {
            const string reply = "Pong....\nWithin {0} ms";
            
            await Context.Channel.EmbedAsync(new EmbedBuilder()
                .WithColor(9896005)
                .WithDescription(string.Format(reply, Context.Client.Latency)));
        }

        [
            Command("remind"),
            Summary("Reminds you of a text after a defined time period."),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> HandleRemindInput(params string[] arguments)
        {
            if(arguments.Length < 1)
                return CustomResult.FromError("The correct usage is `;remind <duration> <text>`");

            string durationStr = arguments[0];


            TimeSpan span = Extensions.GetTimeSpanFromString(durationStr);
            DateTime targetTime = DateTime.Now.Add(span);

            string reminderText;
            if(arguments.Length > 1)
            {
                string[] reminderParts = new string[arguments.Length -1];
                Array.Copy(arguments, 1, reminderParts, 0, arguments.Length - 1);
                reminderText = string.Join(" ", reminderParts);
            } 
            else 
            {
                return CustomResult.FromError("You need to provide a text.");
            }

            var author = Context.Message.Author;

            var guild = Context.Guild;

            var reminder = new Reminder();
            reminder.RemindText = Extensions.RemoveIllegalPings(reminderText);
            reminder.RemindedUserId = author.Id;
            reminder.TargetDate = targetTime;
            reminder.ReminderDate = DateTime.Now;
            reminder.ChannelId = Context.Channel.Id;
            reminder.MessageId = Context.Message.Id;
            
            using(var db = new Database())
            {
                db.Reminders.Add(reminder);
                db.SaveChanges();
                if(targetTime <= DateTime.Now.AddMinutes(60))
                {
                    var difference = targetTime - DateTime.Now;
                    ReminderTimerManger.RemindUserIn(author.Id, difference, reminder.ID);
                    reminder.ReminderScheduled = true;
                }
               
                db.SaveChanges();
            }

            await Context.Channel.SendMessageAsync($"{Context.User.Mention} Scheduled reminder id {reminder.ID}.");

            return CustomResult.FromSuccess();
        }

        [
            Command("unremind"),
            Summary("Cancells the reminder by id."),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> HandleUnRemindInput(ulong reminderId)
        {
            await Task.CompletedTask;
            using(var db = new Database())
            {
                var reminder = db.Reminders.Where(re => re.ID == reminderId && re.RemindedUserId == Context.User.Id).FirstOrDefault();
                if(reminder != null)
                {
                    reminder.Reminded = true;
                    db.SaveChanges();
                    return CustomResult.FromSuccess();
                }
                else
                {
                    return CustomResult.FromError("Reminder not known or not started by you.");
                }
            }
        }

        [
            Command("rank"),
            Summary("Shows your/another users experience, level, and rank in the server"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ShowLevels([Optional] IGuildUser user)
        {
            IUser userToUse = null;
            if(user != null)
            {
                userToUse = user;
            }
            else 
            {
                userToUse = Context.Message.Author;
            }
            if(userToUse.IsBot)
            {
                return CustomResult.FromIgnored();
            }
            var embedBuilder = new EmbedBuilder();
            using(var db = new Database())
            {
                var userInDb = db.Users.Where(us => us.Id == userToUse.Id).FirstOrDefault();
                if(userInDb != null)
                {
                    var rank = db.Users.OrderByDescending(us => us.XP).ToList().IndexOf(userInDb) + 1;
                    var nextLevel = db.ExperienceLevels.Where(lv => lv.Level == userInDb.Level + 1).FirstOrDefault();
                    embedBuilder.WithAuthor(new EmbedAuthorBuilder().WithIconUrl(userToUse.GetAvatarUrl()).WithName(userToUse.Username));
                    embedBuilder.AddField("XP", userInDb.XP, true);
                    embedBuilder.AddField("Level", userInDb.Level, true);
                    embedBuilder.AddField("Messages", userInDb.MessageCount, true);
                    if(nextLevel != null)
                    {
                        embedBuilder.AddField("XP to next Level", nextLevel.NeededExperience - userInDb.XP, true);
                    }
                    embedBuilder.AddField("Rank", rank, true);
                }
                else 
                {
                    embedBuilder.WithTitle("No experience tracked.").WithDescription("Please check back in a minute.");
                }
            }
            await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
            return CustomResult.FromSuccess();
        }

        [
            Command("leaderboard"),
            Summary("shows the top page of the leaderboard (or a certain page)"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ShowLeaderboard([Optional] int page)
        {
            var embedBuilder = new EmbedBuilder();
            using(var db = new Database())
            {
                var allUsers = db.Users.OrderByDescending(us => us.XP).ToList();
                var usersInLeaderboard = db.Users.OrderByDescending(us => us.XP);
                System.Collections.Generic.List<User> usersToDisplay;
                if(page > 1)
                {
                    usersToDisplay = usersInLeaderboard.Skip((page -1) * 10).Take(10).ToList();
                }
                else 
                {
                    usersToDisplay = usersInLeaderboard.Take(10).ToList();
                }
                embedBuilder = embedBuilder.WithTitle("Leaderboard of gained experience");
                var description = new StringBuilder();
                if(page * 10 > allUsers.Count())
                {
                    description.Append("Page not found. \n");
                }
                else
                {
                    description.Append("Rank | Name | Experience | Level | Messages \n");
                    foreach(var user in usersToDisplay)
                    {
                        var rank = allUsers.IndexOf(user) + 1;
                        var userInGuild = Context.Guild.GetUser(user.Id);
                        var name = userInGuild != null ? Extensions.FormatUserName(userInGuild) : "User left guild " + user.Id;
                        description.Append($"[#{rank}] → **{name}**\n");
                        description.Append($"XP: {user.XP} Level: {user.Level}: Messages: {user.MessageCount} \n \n");
                    }
                    description.Append("\n");
                }
               
                description.Append("Your placement: \n");
                var caller = db.Users.Where(us => us.Id == Context.Message.Author.Id).FirstOrDefault();
                if(caller != null)
                {
                    var callRank = allUsers.IndexOf(caller) + 1;
                    var userInGuild = Context.Guild.GetUser(caller.Id);
                    description.Append($"[#{callRank}] → *{Extensions.FormatUserName(userInGuild)}* XP: {caller.XP} messages: {caller.MessageCount} \n");
                    description.Append($"Level: {caller.Level}");
                }
                embedBuilder = embedBuilder.WithDescription(description.ToString());
                embedBuilder.WithFooter(new EmbedFooterBuilder().WithText("Use leaderboard <page> to view more of the leaderboard"));
                await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
               
            }
           
            return CustomResult.FromSuccess();
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


       /* [Command("timeleft")]
        [Summary("How long until the 7 Launch event.")]
        public async Task TimeleftAsync()
        {
            try
            {

                DateTime daysLeft = DateTime.Parse("2019-05-14T15:00:00");
                DateTime startDate = DateTime.UtcNow;

                TimeSpan t = daysLeft - startDate;
                string countDown = string.Format("{0} Days, {1} Hours, {2} Minutes, {3} Seconds until launch.", t.Days, t.Hours, t.Minutes, t.Seconds);
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription(countDown));
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }

        } */
    }
}
