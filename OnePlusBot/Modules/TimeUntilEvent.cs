using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using System.Globalization;


namespace OnePlusBot.Modules
{
   public class TimeLeft : ModuleBase<SocketCommandContext>
    {
        [Command("timeleft")]
        [Summary("How much time until the 6T Launch event.")]
        public async Task TimeleftAsync()
        {
            try
            {

                DateTime daysLeft = DateTime.Parse("2018-10-30T16:00:00+02:00");
                DateTime startDate = DateTime.UtcNow;

                TimeSpan t = daysLeft - startDate;
                string countDown = string.Format("{0} Days, {1} Hours, {2} Minutes, {3} Seconds until launch.", t.Days, t.Hours, t.Minutes, t.Seconds);
                await ReplyAsync(countDown);
            }
            catch(Exception ex)
            {
                await ReplyAsync(ex.Message);
            }

        }
    }
}
