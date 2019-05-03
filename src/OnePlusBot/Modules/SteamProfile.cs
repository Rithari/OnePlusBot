using Discord.Commands;
using System;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class SteamModule : ModuleBase<SocketCommandContext>
    {
        [Command("steamp")]
        [Summary("Steam profile banner for Discord!")]
        public async Task SteampAsync([Remainder] string user)
        {
            if (Regex.IsMatch(Context.Message.Content, @"\;steamp\s+\w+"))
            {
                try //Try and create a banner
                {
                    //Inherit idisposable with webclient instance
                    using (WebClient download = new WebClient())
                    {
                        //Split the input of the user to get the name of the person
                        string split = Regex.Split(Context.Message.Content, @"steamp\s+")[1];

                        //Download the signature
                        await download.DownloadFileTaskAsync(new Uri("https://badges.steamprofile.com/profile/default/steam/" + split + ".png"), "steam.png");

                        //Load the steam png into a new bitmap image
                        Bitmap f = new Bitmap("steam.png");

                        //Check if the pixels at x width y height are a certain color
                        //This is to determine if the picture says it couldnt find a user
                        if (f.GetPixel(115, 41).R == 242)
                        {
                            //Return an error message saying the user does not exist
                            await ReplyAsync("That user does not exist, try to use the custom profile link instead.");
                        }

                        //If the user exists
                        else
                        {
                            //Send the file
                            await Context.Channel.SendFileAsync("steam.png");
                        }

                        //Dispose the bitmap memory
                        f.Dispose();
                    }
                }
                //If the program downloads to much it will raise exceptions
                //Catch it and report the error to the user
                catch (Exception ex)
                {
                    //Send error message
                    Console.WriteLine(ex.Message);
                    await ReplyAsync("An error occured, for debugging purpose check Console.");
                }
            }
            //If they entered it correctly
            else
            {
                await ReplyAsync("Incorrect format, use it like this: `;steamp name`.");
            }
        }
    }
}
