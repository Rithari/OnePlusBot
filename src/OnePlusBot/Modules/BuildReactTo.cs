using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;

namespace OnePlusBot.Modules
{
    public class BuildReactModule : ModuleBase<SocketCommandContext>
    {
        [Command("buildreact")]
        [Summary("Build a message that logs reactions added, hardcoded.")]
        [RequireOwner]

        public async Task BuildReactToAsync()
        {
            RestUserMessage reactmsg = await Context.Channel.SendMessageAsync("React to this message.");
           // await reactmsg.AddReactionsAsync(new Emoji[] { new Emoji(""), new Emoji("") })
            await reactmsg.AddReactionsAsync(new Emoji[] { new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣"), new Emoji("5️⃣"), new Emoji("6️⃣"), new Emoji("7️⃣"), new Emoji("8️⃣"), new Emoji("9️⃣") });
            Global.ReactBuilderMsgId = reactmsg.Id;
        }

    }
}
