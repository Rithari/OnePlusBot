using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using OnePlusBot.Base;

namespace OnePlusBot.Modules
{
    public class BuildModReact : ModuleBase<SocketCommandContext>
    {
        [Command("modroles", RunMode = RunMode.Async)]
        [Summary("Build a message that logs reactions added, hardcoded.")]
        [RequireOwner]

        public async Task BuildReactToAsync()
        {
            var emote1 = new Emoji("📰");

            RestUserMessage modreactmsg = await Context.Channel.SendMessageAsync("**Self Assignable Roles**" +
                "\n\n__You can assign yourself one of the following roles by reacting to its corresponding emote.__\n" +
                "\n" + emote1 + " Journalist" +
                "\n\n *To leave a role, remove your reaction. Spam will be punished by ban until countermeasures are in place.*");

            Global.ReactBuilderModMsgId = modreactmsg.Id;

            await modreactmsg.AddReactionsAsync(new Emoji[]
              {new Emoji("📰") });

        }

    }
}
