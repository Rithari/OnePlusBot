using Discord.Commands;
using System.Threading.Tasks;
using Discord;

namespace OnePlusBot.Modules
{
    public class EchoModule : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Echoes back the remainder argument of the command.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public Task EchoAsync([Remainder] string text)
        {
            return ReplyAsync(text);
        }
    }
}
