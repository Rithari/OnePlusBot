using System;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using System.Text.RegularExpressions;

namespace OnePlusBot.Modules
{
    public class CheckNamesModule : ModuleBase<SocketCommandContext>
    {/*
        [Command("checknames")]
        [Summary("Checks for illegal usernames / nicknames.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task CheckAsync()
        {
            try
            {
                var users = Context.Guild.Users.Where(x => (!Regex.IsMatch(x.Username, @"^[A-Z0-9_-]+$", RegexOptions.IgnoreCase))); //.Select(x => x.Mention);
                string usernames = String.Join("\n", users);
                string[] splitted = usernames.SplitByLength().ToArray();

                await ReplyAsync(splitted[0]);
                await ReplyAsync(splitted[1]);
                //await ReplyAsync(splitted[2]);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
                           Should make this work at some point.
        }*/

       
    }
}
