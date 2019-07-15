using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using OnePlusBot.Base;
using MySql.Data.MySqlClient;

namespace OnePlusBot.Modules
{
    public class Sql : ModuleBase<SocketCommandContext>
    {
        [Command("sql")]
        [Summary("Executes the MySQL command.")]
        [RequireOwner]
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
                await Context.Message.AddReactionAsync(Global.OnePlusEmote.SUCCESS);
            }
            catch
            {
                await Context.Message.AddReactionAsync(new Emoji("\u274C"));
            }
        }
    }
}