using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;

namespace OnePlusBot.Modules
{
    public class FAQModule : ModuleBase<SocketCommandContext>
    {
        [Command("faq")]
        [Summary("Answers frequently asked questions with a predetermined response.")]
        public async Task FAQAsync([Remainder] string parameter)
        {
            try
            {
                switch (parameter.ToLower())
                {
                    case "blu_spark":
                    case "bluspark":
                        if (Context.Channel.Name == "oneplus6")
                        {
                            await ReplyAsync("blu_spark for OnePlus 6: https://forum.xda-developers.com/oneplus-6/development/kernel-t3800965");
                        }
                        if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await ReplyAsync("blu_spark for OnePlus 5: https://forum.xda-developers.com/oneplus-5/development/kernel-t3651933" + Environment.NewLine +
                                             "blu_spark for OnePlus 5T: https://forum.xda-developers.com/oneplus-6/development/kernel-t3800965");
                        }
                        if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await ReplyAsync("blu_spark for OnePlus 3 & 3T: https://forum.xda-developers.com/oneplus-3/oneplus-3--3t-cross-device-development/kernel-t3404970");
                        }
                        if (Context.Channel.Name == "legacy")
                        {
                            await ReplyAsync("blu_spark for OnePlus X: https://forum.xda-developers.com/oneplus-x/orig-development/kernel-t3250995");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}
