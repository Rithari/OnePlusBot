using System;
using Discord;
using Discord.Commands;
using OnePlusBot.Helpers;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace OnePlusBot.Properties
{
    public class ServerInfo : ModuleBase<SocketCommandContext>
    {
        public readonly DiscordSocketClient _client;

        [Command("serverinfo")]
        [Summary("Shows server information.")]
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
    }
}
