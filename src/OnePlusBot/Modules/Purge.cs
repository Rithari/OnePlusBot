using System;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using OnePlusBot.Base;


namespace OnePlusBot.Modules
{
    public class PurgeModule : ModuleBase<SocketCommandContext>
    {
        [Command("purge", RunMode = RunMode.Async)]
        [Summary("Deletes specified amount of messages.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task<RuntimeResult> PurgeAsync([Remainder] double delmsg)
        {
            if (delmsg > 100 || delmsg <= 0)
                return CustomResult.FromError("Use a number between 1-100");

            int delmsgInt = (int)delmsg;
            ulong oldmessage = Context.Message.Id;

            // Download all messages that the user asks for to delete
            var messages = await Context.Channel.GetMessagesAsync(oldmessage, Direction.Before, delmsgInt).FlattenAsync();
            await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messages);

            // await Context.Message.DeleteAsync();


            return CustomResult.FromSuccess(); //This will generate an error, because we delete the message, TODO: Implement a workaround.



        }
    }
}

