using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using OnePlusBot._Extensions;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Modules
{
    public class UpdateModule : InteractiveBase<SocketCommandContext>
    {
        [Command("update")]
        [Summary("Updates the database with the current state of the server.")]
        [RequireOwner]
        public async Task UpdateAsync()
        {
            var guild = Context.Guild;
            if (guild.Id != Global.ServerID)
                return;

            bool updated = false;
            var builder = new StringBuilder();        
            using (var db = new Database())
            {
                var dbChannels = db.Channels.ToArray();
                foreach (var channel in guild.Channels)
                {
                    if (dbChannels.Any(x => x.ID == channel.Id))
                        return;

                    var question = await Context.Channel.EmbedAsync(new EmbedBuilder()
                        .WithTitle("Provide channel's entry name")
                        .WithColor(9896005)
                        .WithDescription($"We are adding channel <#{channel.Id}> to the database.\n" +
                                         "What ID would you like to use?\n" +
                                         $"Suggested `{CleanupChannelName(channel.Name)}`"));
                    
                    var reply = await NextMessageAsync(
                            new EnsureFromUserCriterion(Context.Message.Author.Id),
                            TimeSpan.FromSeconds(30));

                    var newName = reply != null ? CleanupChannelName(reply.Content) : CleanupChannelName(channel.Name);
                    
#pragma warning disable 4014
                    question.DeleteAsync();
                    reply?.DeleteAsync();
#pragma warning restore 4014

                    db.Channels.Add(new Channel
                    {
                        Name = newName,
                        ChannelID = channel.Id
                    });
                    
                    builder.AppendFormat("[+] Added <#{0}> channel\n", channel.Id);
                    updated = true;
                }

                foreach (var channel in dbChannels)
                {
                    if (guild.Channels.Any(x => x.Id == channel.ID))
                        return;
                    
                    db.Channels.Remove(channel);
                    
                    builder.AppendFormat("[-] Removed `{0}` channel", channel.Name);
                    updated = true;
                }
            }

            var embed = new EmbedBuilder();
            embed.Title = updated 
                ? "Updated Database" 
                : "Database is up-to-date";
            embed.WithColor(9896005);

            embed.Description = updated 
                ? "Database was successfully updated."
                : "Database was already up-to-date.";

            embed.Fields.Add(new EmbedFieldBuilder()
                .WithName("Changes")
                .WithValue(builder.ToString()));
            
            await Context.Channel.EmbedAsync(embed);
        }

        /// <summary>
        /// Cleanups the channel name
        /// </summary>
        /// <returns>The channel name all lowercase without special characters.</returns>
        private static string CleanupChannelName(string name)
        {
            var builder = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                if (char.IsLetter(c))
                {
                    if (char.IsUpper(c))
                        builder.Append(c + ' ');
                    else
                        builder.Append(c);
                }
            }
            return builder.ToString();
        }
    }
}
