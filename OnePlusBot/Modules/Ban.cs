using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans copy pasters")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser user,[Remainder] string reason = null)
        {
            if(reason == null)
            {
                reason = "No reason provided.";
            } 
            try
            {
               /* if(user.Id == Context.Message.Author.Id)
                {
                    var EmoteFalse = new Emoji("❌");
                    await Context.Message.AddReactionAsync(EmoteFalse);
                    await ReplyAsync("you are tring to ban yourself, fuck off.");
                    return;

                } */
                var EmoteTrue = new Emoji(":success:499567039451758603");
                await Context.Message.AddReactionAsync(EmoteTrue);
                //await ReplyAsync(user.Mention + " settled.");
                await Context.Guild.AddBanAsync(user, 0, reason);

            }
            catch (Exception ex)
            {
                //   await ReplyAsync(ex.Message);
                var EmoteFalse = new Emoji("❌");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(EmoteFalse);
                await ReplyAsync("(Un)fortunately you can not ban staff.");
            }
        }
    }
}
