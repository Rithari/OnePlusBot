﻿using System;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class PingModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Standard ping command.")]
        public async Task PingAsync()
        {
            const string reply = "Pong....\nWithin {0} ms";
            
            await Context.Channel.EmbedAsync(new EmbedBuilder()
                .WithColor(9896005)
                .WithDescription(string.Format(reply, Context.Client.Latency)));
        }
    }
}
