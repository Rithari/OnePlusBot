using System;
using System.Text;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using OnePlusBot.Helpers;
using Discord.Addons.Interactive;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using System.Linq;
using MySql.Data.MySqlClient;

namespace OnePlusBot.Modules
{
    [
      Summary("Commands to manage the bot. Exclusive to the owner of the bot.")
    ]
    public class Owner : InteractiveBase<SocketCommandContext>
    {

        [
            Command("x"),
            Summary("Emergency shutdown of the bot."),
            RequireOwner
        ]
        public async Task ShutdownAsync()
        {
            await Context.Message.AddReactionAsync(StoredEmote.GetEmote(Global.OnePlusEmote.SUCCESS));
            Environment.Exit(0);
        }

        [
            Command("sql"),
            Summary("Executes the MySQL command."),
            RequireOwner
        ]
        public async Task ExecuteQuery([Remainder] string query)
        {
            var server = Environment.GetEnvironmentVariable("Server");
            var db = Environment.GetEnvironmentVariable("Database");
            var uid = Environment.GetEnvironmentVariable("Uid");
            var pwd = Environment.GetEnvironmentVariable("Pwd");
            
            if (server == null || db == null || uid == null || pwd == null)
                throw new Exception("Cannot find MySQL connection string in EnvVar.");
            
            var connStr = new MySqlConnectionStringBuilder
            {
                Server = server,
                Database = db,
                UserID = uid,
                Password = pwd
            };

            try
            {
                using (var connection = new MySqlConnection())
                {
                    connection.ConnectionString = connStr.ToString();
                    connection.Open();

                    using (var sql = new MySqlCommand(query, connection))
                    {
                        await sql.ExecuteNonQueryAsync();
                    }
                }
                await Context.Message.AddReactionAsync(StoredEmote.GetEmote(Global.OnePlusEmote.SUCCESS));
            }
            catch
            {
                await Context.Message.AddReactionAsync(new Emoji("\u274C"));
            }
        }

         private async Task<string> Ask(string title, string adding, string suggested)
        {
            var question = await Context.Channel.EmbedAsync(new EmbedBuilder()
                .WithTitle(title)
                .WithColor(9896005)
                .WithDescription($"We are adding {adding} to the database.\n" +
                                 "What ID would you like to use?\n" +
                                 $"Suggested `{suggested}`"));

            var reply = await NextMessageAsync(
                new EnsureFromUserCriterion(Context.Message.Author.Id),
                TimeSpan.FromSeconds(10));

            var input = reply?.Content;
            
#pragma warning disable 4014
            question.DeleteAsync();
            reply?.DeleteAsync();
#pragma warning restore 4014
            
            if (input == "cancel")
                return null;

            if (input == null || input.Equals("y", StringComparison.OrdinalIgnoreCase))
                return suggested;
            
            return CleanupEntryName(reply.Content);
        }
        
        [
            Command("update", RunMode = RunMode.Async),
            Summary("Updates the database with the current state of the server."),
            RequireOwner
        ]
        public async Task UpdateAsync()
        {
            var guild = Context.Guild;
            if (guild.Id != Global.ServerID)
                return;

            int additions = 0;
            int removals = 0;
            using (var db = new Database())
            {
                foreach (var channel in guild.TextChannels)
                {
                    if (db.Channels.Any(x => x.ChannelID == channel.Id))
                        continue;

                    var newName = await Ask(
                        "Provide channel's entry name",
                        $"channel <#{channel.Id}>",
                        CleanupEntryName(channel.Name));

                    if (newName == null)
                        return;

                    db.Channels.Add(new Channel
                    {
                        Name = newName,
                        ChannelID = channel.Id,
                        ChannelType = ChannelType.Text
                    });

                    Console.WriteLine($"[DB] [Update] Added channel {channel.Name} with Name {newName} and ID {channel.Id}");
                    additions++;
                }

                var textChannels = db.Channels.AsQueryable().Where(x => x.ChannelType == ChannelType.Text);
                if (textChannels.Any())
                {
                    foreach (var channel in textChannels)
                    {
                        if (guild.Channels.Any(x => x.Id == channel.ChannelID))
                            continue;

                        db.Channels.Remove(channel);

                        Console.WriteLine($"[DB] [Update] Removed channel {channel.Name} with ID {channel.ChannelID}");
                        removals--;
                    }
                }

                foreach (var role in guild.Roles)
                {
                    if (role.IsEveryone || role.IsManaged)
                        continue;
                    
                    if (db.Roles.Any(x => x.RoleID == role.Id))
                        continue;

                    var newName = await Ask(
                        "Provide role's entry name",
                        $"role {role.Name}",
                        CleanupEntryName(role.Name));

                    if (newName == null)
                        return;

                    db.Roles.Add(new Role
                    {
                        Name = newName,
                        RoleID = role.Id,
                        XPRole = false
                    });

                    Console.WriteLine($"[DB] [Update] Added role {role.Name} with Name {newName} and ID {role.Id}");
                    additions++;
                }

                if (db.Roles.Any())
                {
                    foreach (var role in db.Roles)
                    {
                        if (guild.Roles.Any(x => x.Id == role.RoleID))
                            continue;

                        db.Roles.Remove(role);

                        Console.WriteLine($"[DB] [Update] Removed role {role.Name} with ID {role.RoleID}");
                        removals--;
                    }
                }
                
                db.SaveChanges();
            }

            var embed = new EmbedBuilder();
            embed.Title = additions + removals > 0
                ? "Updated Database"
                : "Database is up-to-date";
            embed.WithColor(9896005);

            embed.Description = additions + removals > 0
                ? "Database was successfully updated."
                : "Database was already up-to-date.";
            
            embed.Fields.Add(new EmbedFieldBuilder()
                .WithName("Changes")
                .WithValue($"We added {additions} elements to the database and removed {removals} obsolete entries."));

            await Context.Channel.EmbedAsync(embed);
        }

        private static bool IsAscii(char c)
        {
            return 'A' <= c && c <= 'Z' ||
                   'a' <= c && c <= 'z';
        }
        
        /// <summary>
        /// Cleanups the channel name
        /// </summary>
        /// <returns>The channel name all lowercase without special characters.</returns>
        private static string CleanupEntryName(string name)
        {
            var builder = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                if (IsAscii(c))
                {
                    if (char.IsUpper(c))
                        builder.Append(char.ToLowerInvariant(c));
                    else
                        builder.Append(c);
                }
            }
            return builder.ToString();
        }


    }
}
