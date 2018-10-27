﻿using System;
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
        [Summary("Bans specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser user,[Remainder] string reason = null)
        {
            if (user.IsBot)
            {
                var EmoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(EmoteFalse);
                await ReplyAsync("You humans can't make us harm each other.");
            }
            if(reason == null)
            {
                reason = "No reason provided.";
            } 
            try
            {
                var EmoteTrue = new Emoji(":success:499567039451758603");
                await Context.Message.AddReactionAsync(EmoteTrue);
                await Context.Guild.AddBanAsync(user, 0, reason);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var EmoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(EmoteFalse);
                await ReplyAsync("You can not ban staff.");
            }
        }
    }
}
