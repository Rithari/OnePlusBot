using System;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using OnePlusBot._Extensions;

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
                    case "twrp":
                        if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus 6: <https://forum.xda-developers.com/oneplus-6/development/kernel-t3800965>" + Environment.NewLine +
                                             "blu_spark XDA thread for OnePlus 6T: <https://forum.xda-developers.com/oneplus-6t/development/kernel-t3861123>" + Environment.NewLine +
                                             "Download link for 6 and 6T: <https://github.com/engstk/android_device_oneplus_enchilada/releases>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus 5: <https://forum.xda-developers.com/oneplus-5/development/kernel-t3651933>" + Environment.NewLine +
                                             "blu_spark XDA thread for OnePlus 5T: <https://forum.xda-developers.com/oneplus-5t/development/kernel-t3706295>" + Environment.NewLine +
                                             "Download link for 5 and 5T: <https://github.com/engstk/android_device_oneplus_cheeseburger/releases>"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark for OnePlus 3 & 3T: <https://forum.xda-developers.com/oneplus-3/oneplus-3--3t-cross-device-development/kernel-t3404970>"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark for OnePlus X: <https://forum.xda-developers.com/oneplus-x/orig-development/kernel-t3250995>"));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please perform your request in the correct device channel."));
                        }
                        break;
                    case "unbrick":
                    case "bricked":
                    case "brick":
                       if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unbrick guide for OnePlus 6: <https://forums.oneplus.com/threads/guide-mega-unbrick-guide-for-a-hard-bricked-oneplus-6.840709>" + Environment.NewLine + 
                                             "Unbrick guide for OnePlus 6T: <https://forum.xda-developers.com/oneplus-6t/how-to/tool-6t-msmdownloadtool-v4-0-oos-9-0-5-t386744>" + Environment.NewLine +
                                             "Unbrick guide for OnePlus 6T TMobile: <https://forum.xda-developers.com/oneplus-6t/how-to/tool-t-mobile-oneplus-6t-msmdownloadtool-t3868916>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unbrick tool for OnePlus 5: <https://forums.oneplus.com/threads/guide-mega-unbrick-guide-for-a-hard-bricked-oneplus-5.594957>" + Environment.NewLine +
                                             "Unbrick tool for OnePlus 5T: <https://forums.oneplus.com/threads/guide-mega-unbrick-guide-for-a-hard-bricked-oneplus-5t.751497>"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unbrick tool for OnePlus 3: <https://forums.oneplus.com/threads/guide-mega-unbrick-guide-for-a-hard-bricked-oneplus-3.452634>" + Environment.NewLine +
                                             "Unbrick tool for OnePlus 3T: <https://forums.oneplus.com/threads/guide-oneplus-3-3t-unbrick.531047>"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unbrick guide for OnePlus 2: <https://forums.oneplus.com/threads/updated-28-06-2016-mega-unbrick-guide-for-a-hard-bricked-oneplus-2.347607>" + Environment.NewLine +
                                             "Unbrick guide for OnePlus X: <https://forums.oneplus.com/threads/guide-mega-unbrick-guide-for-a-hard-bricked-oneplus-x.417648>" + Environment.NewLine +
                                             "Unbrick guide for OnePlus One: <https://forum.xda-developers.com/oneplus-one/general/guide-unbrick-oneplus-one-t3013732>"));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please perform your request in the correct device channel."));
                        }
                        break;
                    case "root":
                    case "magisk":
                        if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Flash magisk zip using TWRP img (`fastboot boot twrp.img`) as TWRP is not meant to be flashed on A/B devices (see <https://twitter.com/topjohnwu/status/1070029212428439553> )"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Flash magisk zip after installing TWRP (`fastboot flash recovery twrp.img` and `fasboot reboot`) "));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Flash magisk zip after installing TWRP (`fastboot flash recovery twrp.img` and `fasboot reboot`) "));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Flash magisk zip after installing TWRP (`fastboot flash recovery twrp.img` and `fasboot reboot`) "));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please refer to your device channel for detailled instructions. As a reminder, TWRP is not meant to be flashed on A/B devices (see <https://twitter.com/topjohnwu/status/1070029212428439553> )"));
                        }
                        break;
                    case "gcam":
                    case "googlecam":
                    case "googlecamera":
                        if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Google Camera for OnePlus 6 & 6T: <https://forum.xda-developers.com/oneplus-6/themes/oneplus-6-google-camera-port-t3797544>" + Environment.NewLine +
                                             "For all device APKs, please visit: <https://www.celsoazevedo.com/files/android/google-camera>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Google Camera for OnePlus 5 & 5T: <https://forum.xda-developers.com/oneplus-5/themes/google-camera-hdr-t3655215>" + Environment.NewLine +
                                             "For all device APKs, please visit: <https://www.celsoazevedo.com/files/android/google-camera>"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Google Camera for 3 & 3T: <https://forum.xda-developers.com/oneplus-3/how-to/modded-google-camera-hdr-60fps-video-t3658552>" + Environment.NewLine +
                                             "For all device APKs, please visit: <https://www.celsoazevedo.com/files/android/google-camera>"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Google Camera only supports 64 bit devices. Sorry."));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("For all device APKs, please visit: <https://www.celsoazevedo.com/files/android/google-camera>"));
                        }
                        break;
                    case "oxygenos":
                    case "oosmirror":
                    case "oos":
                    case "ota":
                    case "rom":
                        if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("OnePlus 6 mirrors for official OxygenOS: <https://forums.oneplus.com/threads/oneplus-6-rom-ota-oxygen-os-mirrors-for-official-oxygen-os-roms-and-ota-updates.835607>" + Environment.NewLine +
                                             "OnePlus 6T mirrors unavailable. Please use <https://www.oneplus.com/support/softwareupgrade/details?code=9>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("OnePlus 5 mirrors for official OxygenOS: <https://forums.oneplus.com/threads/oneplus-5-oxygenos-5-1-4-official-download-links.557183>" + Environment.NewLine +
                                             "OnePlus 5T mirrors for official OxygenOS: <https://forums.oneplus.com/threads/oneplus-5t-rom-ota-oxygen-os-mirrors-for-official-oxygen-os-roms-and-ota-updates.686610>"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("OnePlus 3T mirrors unavailable. Please use <https://www.oneplus.com/support/softwareupgrade/details?code=6>" + Environment.NewLine +
                                             "OnePlus 3 mirrors for official OxygenOS: <https://forums.oneplus.com/threads/latest-3-2-4-oneplus-3-mirrors-for-official-oxygen-os-roms-and-ota-updates.450783>"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("OnePlus X mirrors unavailable. Please use <https://www.oneplus.com/support/softwareupgrade/details?code=3>" + Environment.NewLine +
                                             "OnePlus 2 mirrors for official OxygenOS: <https://forum.xda-developers.com/oneplus-2/general/rom-mirrors-official-oxygen-os-roms-ota-t3209863>" + Environment.NewLine +
                                             "OnePlus One mirrors unavailable. Please use <https://www.oneplus.com/support/softwareupgrade/details?code=1>"));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("For all device APKs, please visit: <https://www.celsoazevedo.com/files/android/google-camera>"));
                        }
                        break;
                    case "edl":
                    case "9008":
                    case "9008mode":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("To enter in Qualcomm EDL mode, you can use `adb reboot edl` or use any version of blu_spark TWRP based on TWRP 3.3.0 or later by clicking on `Reboot to EDL`." + Environment.NewLine +
                                             "If you want to exit EDL mode, maintain power button during at least 10 seconds "));
                        }
                        break;
                    case "whiteled":
                    case "qualcommdiagnostics":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("If you have a persistent white LED while trying to boot your phone and a black screen, it means that your device is stuck in Qualcomm Diagnostics mode." + Environment.NewLine +
                                             "You can turn off your phone by maintaining power button during at least 10 seconds and then use MSM tool to recover it to a working state"));
                        }
                        break;
                    case "smt":
                    case "smtdownloadmode":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("If you're reading this, stop what you are doing. Please." + Environment.NewLine +
                                             "The reason why aftersales support of OnePlus use Upgrade Mode in MSM tool during remote assistance sessions rather than SMT mode is that it is meant for factory only as it wipes NV (non volatile) items suh as IMEI (source <https://forum.xda-developers.com/showpost.php?p=77937552&postcount=90>)"));
                        }
                        break;
                    default:
                        await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Supported commands are: bluspark, googlecamera, oxygenos, unbrick, edl, magisk, root"));
                        break;
                }
            }
            catch (Exception ex)
            {
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription(ex.Message));
            }
        }
    }
}
