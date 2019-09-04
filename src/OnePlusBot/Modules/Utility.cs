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
            Command("report"),
            Summary("Suggests something to the server.")
        ]
        public async Task<RuntimeResult> ReportAsync(IGuildUser user, [Optional] [Remainder] string reason)
        {
            var reportChannel = Context.Guild.GetTextChannel(Global.Channels["reports"]);;

            var reporter = Context.Message.Author;

            var entry = new ReportEntry
            {
                ReportedUser = user.Username + '#' + user.Discriminator,
                ReportedUserId = user.Id,
                ReportedBy = reporter.Username + '#' + reporter.Discriminator,
                ReportedById = reporter.Id,
                Reason = reason,
                ChannelID = Context.Channel.Id,
                Date = Context.Message.Timestamp.Date,
            };

            using (var db = new Database())
            {
                db.Reports.Add(entry);
                db.SaveChanges();
            }

            var builder = new EmbedBuilder();
            builder.Title = "...a new report has emerged from outer space!";
            builder.Color = new Color(0x3E518);
            
            builder.Timestamp = Context.Message.Timestamp;
            
            builder.WithFooter(footer =>
            {
                footer
                    .WithText("Case #" + entry.ID)
                    .WithIconUrl("https://a.kyot.me/0WPy.png");
            });
            
            builder.ThumbnailUrl = user.GetAvatarUrl();
            
            builder.WithAuthor(author =>
            {
                author
                    .WithName("Woah...")
                    .WithIconUrl("https://a.kyot.me/cno0.png");
            });
            var reportedUserSafe = Extensions.FormatMentionDetailed(user);
            var reporterUserSafe = Extensions.FormatMentionDetailed(reporter);

            const string discordUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
            builder.AddField("Reported User", reportedUserSafe)
                .AddField("Reported by", reporterUserSafe)
                .AddField(
                    "Location of the incident",
                    $"[#{Context.Message.Channel.Name}]({string.Format(discordUrl, Context.Guild.Id, Context.Channel.Id, Context.Message.Id)})")
                .AddField("Reason", reason ?? "No reason was provided.");


            var embed = builder.Build();
            await reportChannel.SendMessageAsync(null,embed: embed).ConfigureAwait(false);
            return CustomResult.FromSuccess();
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
                embed.AddField(fb =>
                    fb.WithName("Custom emojis" + $"({guild.Emotes.Count})")
                    .WithValue(string.Join(" ", guild.Emotes
                        .Take(20)
                        .Select(e => $"{e.ToString()}"))));
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
