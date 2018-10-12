using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class PingModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task PingAsync()
        {
            var timestamp = Context.Message.Timestamp;
            var ping = DateTime.UtcNow - timestamp;

            var sentmessage = await ReplyAsync("Pong!");
            await sentmessage.ModifyAsync(m => m.Content = "Pong....\nIn " + ping.Milliseconds + " ms");
        }
    }
}
