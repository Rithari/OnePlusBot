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

            var emote1 = new Emoji(":1_:574655515586592769");
            var emote2 = new Emoji(":2_:574655515548844073");
            var emote3 = new Emoji(":X_:574655515481866251");
            var emote4 = new Emoji(":3_:574655515452506132");
            var emote5 = new Emoji(":3T:574655515846508554");
            var emote6 = new Emoji(":5_:574655515745976340");
            var emote7 = new Emoji(":5T:574655515494318109");
            var emote8 = new Emoji(":6_:574655515615952896");
            var emote9 = new Emoji(":6T:574655515846508573");
            var emote10 = new Emoji("❓");
            var emote11 = new Emoji("📰");

            RestUserMessage reactmsg = await Context.Channel.SendMessageAsync("**Self Assignable Roles**" +
                "\n\n__You can assign yourself one of the following roles by reacting to its corresponding emote.__\n" +
                "\n" + emote1 + "OnePlus One" +
                "\n" + emote2 + " OnePlus 2" +
                "\n" + emote3 + " OnePlus X" +
                "\n" + emote4 + " OnePlus 3" +
                "\n" + emote5 + " OnePlus 3T" +
                "\n" + emote6 + " OnePlus 5" +
                "\n" + emote7 + " OnePlus 5T" +
                "\n" + emote8 + " OnePlus 6" +
                "\n" + emote9 + " OnePlus 6T" +
                "\n" + emote10 + " Helper" +
                "\n" + emote11 + " News" +
                "\n\n *To leave a role, remove your reaction. Spam will be punished by ban until countermeasures are in place.*");

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
