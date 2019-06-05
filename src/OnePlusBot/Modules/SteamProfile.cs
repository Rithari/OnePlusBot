using Discord.Commands;
using System;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using OnePlusBot.Base;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Pomelo.EntityFrameworkCore.MySql;

namespace OnePlusBot.Modules
{
    public class SteamModule : ModuleBase<SocketCommandContext>
    {
        private const string BadgeUrl = "https://badges.steamprofile.com/profile/default/steam/{0}.png";
        
        [Command("steamp")]
        [Summary("Steam profile banner for Discord!")]
        public async Task<RuntimeResult> SteampAsync([Remainder] string user)
        {
            await Context.Message.AddReactionAsync(Emote.Parse("<:success:499567039451758603>"));

            var request = (HttpWebRequest) WebRequest.Create(string.Format(BadgeUrl, user));
            
            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                {
                    return CustomResult.FromError("Could not establish a connection to the Steam API");
                }
                
                using (var fs = File.Open("output.png", FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    await stream.CopyToAsync(fs);
                }
                
                await Context.Channel.SendFileAsync("output.png");
            }

            File.Delete("output.png");
            return CustomResult.FromSuccess();
        }
    }
}
