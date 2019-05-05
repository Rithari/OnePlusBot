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
        [Command("buildreact", RunMode = RunMode.Async)]
        [Summary("Build a message that logs reactions added, hardcoded.")]
        [RequireOwner]

        public async Task BuildReactToAsync()
        {
            RestUserMessage reactmsg = await Context.Channel.SendMessageAsync("**Self Assignable Roles**" +
                "\n\n__You can assign yourself one of the following roles by reacting to its corresponding emote.__\n" +
                "\n OnePlus One - \U00000030\U000020e3 " +
                "\n OnePlus 2 - \U00000031\U000020e3" +
                "\n OnePlus X - \U00000032\U000020e3" +
                "\n OnePlus 3 - \U00000033\U000020e3" +
                "\n OnePlus 3T - \U00000034\U000020e3" +
                "\n OnePlus 5 - \U00000035\U000020e3" +
                "\n OnePlus 5T - \U00000036\U000020e3" +
                "\n OnePlus 6 - \U00000037\U000020e3" +
                "\n OnePlus 6T - \U00000038\U000020e3" +
                "\n Helper - ❓" +
                "\n News - 📰" +
                "\n\n *Leaving a role you have joined is currently unsupported and wil be added soon, for now, please contact administration.*");

            Global.ReactBuilderMsgId = reactmsg.Id;
            await reactmsg.AddReactionsAsync(new Emoji[] {new Emoji("\U00000030\U000020e3"), new Emoji("\U00000031\U000020e3"), new Emoji("\U00000032\U000020e3"), new Emoji("\U00000033\U000020e3"), new Emoji("\U00000034\U000020e3"), new Emoji("\U00000035\U000020e3"), new Emoji("\U00000036\U000020e3"), new Emoji("\U00000037\U000020e3"), new Emoji("\U00000038\U000020e3"), new Emoji("❓"), new Emoji("📰") });

        }

    }
}
