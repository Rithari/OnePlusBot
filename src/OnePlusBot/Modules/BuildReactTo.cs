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
                "\n OnePlus One" +
                "\n OnePlus 2" +
                "\n OnePlus X" +
                "\n OnePlus 3" +
                "\n OnePlus 3T" +
                "\n OnePlus 5" +
                "\n OnePlus 5T" +
                "\n OnePlus 6" +
                "\n OnePlus 6T" +
                "\n Helper" +
                "\n News" +
                "\n\n *Leaving a role you have joined is currently unsupported and wil be added soon, for now, please contact administration.*");

            Global.ReactBuilderMsgId = reactmsg.Id;
            await reactmsg.AddReactionsAsync(new Emoji[] 
              { new Emoji(":1_:574655515586592769"),
                new Emoji(":2_:574655515548844073"),
                new Emoji(":X_:574655515481866251"),
                new Emoji(":3_:574655515452506132"),
                new Emoji(":3T:574655515846508554"),
                new Emoji(":5_:574655515745976340"),
                new Emoji(":5T:574655515494318109"),
                new Emoji(":6_:574655515615952896"),
                new Emoji(":6T:574655515846508573"),
                new Emoji("❓"), new Emoji("📰") });

        }

    }
}
