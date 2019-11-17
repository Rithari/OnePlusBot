using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using OnePlusBot.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        [
            Command("showavatar"),
            Summary("Shows avatar of a user.")
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


        [
            Command("userinfo"),
            Summary("Displays User Information")
        ]
        public async Task UserInfo(IGuildUser user = null)
        {
            user = user ?? (IGuildUser) Context.User;
            
            var embed = new EmbedBuilder();

            embed.WithColor(9896005);
            embed.WithAuthor(x =>
            {
                x.Name = user.Username;
            });

            embed.ThumbnailUrl = user.GetAvatarUrl();
            
            embed.AddField(x =>
            {
                x.Name = "Status";
                x.Value = user.Status.ToString();
                x.IsInline = true;
            });
            

            embed.AddField(x =>
            {
                x.Name = "Activity";
                x.Value = user.Activity?.Name ?? "Nothing";
                x.IsInline = true;
            });

            if (user.JoinedAt.HasValue)
            {
                embed.AddField(x =>
                {
                    x.Name = "Joined";
                    x.Value = user.JoinedAt.Value.DateTime.ToString("ddd, MMM dd, yyyy, HH:mm tt", CultureInfo.InvariantCulture);
                    x.IsInline = true;
                });
            }
            
            embed.AddField(x =>
            {
                x.Name = "Registered";
                x.Value = user.CreatedAt.DateTime.ToString("ddd, MMM dd, yyyy, HH:mm tt", CultureInfo.InvariantCulture);
                x.IsInline = true;
            });
            
            
            await Context.Channel.EmbedAsync(embed);
        }

        [
            Command("se"),
            Summary("Shows bigger version of am emote.")
        ]
        public async Task Showemojis([Remainder] string _) // need to have the parameter so that the message.tags gets populated
        {
            var tags = Context.Message.Tags.Where(t => t.Type == TagType.Emoji).Select(t => (Emote)t.Value);
            
            var result = string.Join("\n", tags.Select(m => "**Name:** " + m + " **Link:** " + m.Url));

            if (string.IsNullOrWhiteSpace(result))
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("No special emojis found."));
            else
                await Context.Channel.SendMessageAsync(result).ConfigureAwait(false);
        }

        [
            Command("suggest"),
            Summary("Suggests something to the server.")
        ]
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

        [
            Command("news"),
            Summary("Posts a News article to the server."),
            RequireRole("journalist")
        ]
        public async Task<RuntimeResult> NewsAsync([Remainder] string news)
        {
            var guild = Context.Guild;

            var user = (SocketGuildUser)Context.Message.Author;
            
            var needsAttachments = Context.Message.Attachments.Count() > 0;
            
            var newsChannel = guild.GetTextChannel(Global.Channels["news"]) as SocketNewsChannel;
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

        [
            Command("serverinfo"),
            Summary("Shows server information.")
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
                .AddField(fb => fb.WithName("Created at").WithValue($"{ createdAt:dd.MM.yyyy HH:mm}").WithIsInline(true))
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
            RequireRole("staff")
        ]
        public Task EchoAsync([Remainder] string text)
        {
            return ReplyAsync(text);
        }

        [
            Command("ping"),
            Summary("Standard ping command.")
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
            Summary("Reminds you of a text after a defined time period.")
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
            Summary("Cancells the reminder by id.")
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
