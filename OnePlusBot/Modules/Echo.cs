using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;

namespace OnePlusBot.Modules
{
    public class Echo : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Standard echo function.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public Task EchoAsync([Remainder] string echo)
            => ReplyAsync(echo);
    }
}
