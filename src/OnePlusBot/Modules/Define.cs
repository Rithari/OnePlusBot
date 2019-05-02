using System;
using System.Threading.Tasks;
using Discord.Commands;
using System.Linq;
using System.Net.Http;
using QuickType;

namespace OnePlusBot.Modules
{
    public class DefineModule : ModuleBase<SocketCommandContext>
    {
        private readonly HttpClient _clients;

        public DefineModule(HttpClient clients)
        {
            _clients = clients;
        }


        [Command("define")]
        [Summary("Grabs the first Urban Dictionary result based on the parameter.")]
        public async Task DefineAsync([Remainder] string searchquery)
        {
            try
            {

                var definition = await ReplyAsync("Searching for definitions...");
                

                string json = await _clients.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + searchquery);
                var response = Response.FromJson(json);
                await definition.ModifyAsync(x => x.Content = response.List.FirstOrDefault()?.Definition ?? "No definition found");
            }
            catch(Exception ex)
            {
                await ReplyAsync(ex.Message);
            }

            


        /*   await definition.ModifyAsync(x =>
            {
               // x.Embed
                x.Content = ":notebook_with_decorative_cover:" + "`[Urban Dictionary]`" + Regex.Replace(url, @"&#39;|\<.*?\>|&quot;|&amp;|â€™|â€œ|!â€", "\0");
           }); */


        }


    }
}
