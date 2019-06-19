using System;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using OnePlusBot.Helpers;

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
                    case "repairprices":
                    case "repair":
                    case "repairing":
                        if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus 7: <https://www.oneplus.com/support/repair-pricing/details?code=10>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 7 Pro: <https://www.oneplus.com/support/repair-pricing/details?code=11>"));
                    case "repairprices":
                    case "repair":
                    case "repairing":
                        else if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus 6: <https://www.oneplus.com/support/repair-pricing/details?code=8>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 6T: <https://www.oneplus.com/support/repair-pricing/details?code=9>"));
                    case "repairprices":
                    case "repair":
                    case "repairing":
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus 5: <https://www.oneplus.com/support/repair-pricing/details?code=5>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 5T: <https://www.oneplus.com/support/repair-pricing/details?code=7>"));
                    case "repairprices":
                    case "repair":
                    case "repairing":
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus 3: <https://www.oneplus.com/support/repair-pricing/details?code=4>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 3T: <https://www.oneplus.com/support/repair-pricing/details?code=6>"));
                    case "repairprices":
                    case "repair":
                    case "repairing":
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus One: <https://www.oneplus.com/support/repair-pricing/details?code=1>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 2: <https://www.oneplus.com/support/repair-pricing/details?code=2>" + Environment.NewLine +
                                             "Repair prices for the OnePlus X: <https://www.oneplus.com/support/repair-pricing/details?code=3>"));
                    case "blu_spark":
                    case "bluspark":
                    case "twrp":
                        if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus 6: <https://forum.xda-developers.com/oneplus-6/development/kernel-t3800965>" + Environment.NewLine +
                                             "blu_spark XDA thread for OnePlus 6T: <https://forum.xda-developers.com/oneplus-6t/development/kernel-t3861123>" + Environment.NewLine +
                                             "blu_spark TWRP Download link for OnePlus 6: <https://github.com/engstk/android_device_oneplus_enchilada/releases>" + Environment.NewLine +
                                             "blu_spark TWRP Download link for OnePlus 6T: <https://github.com/engstk/android_device_oneplus_fajita/releases>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus 5: <https://forum.xda-developers.com/oneplus-5/development/kernel-t3651933>" + Environment.NewLine +
                                             "blu_spark XDA thread for OnePlus 5T: <https://forum.xda-developers.com/oneplus-5t/development/kernel-t3706295>" + Environment.NewLine +
                                             "blu_spark TWRP Download link for 5 and 5T: <https://github.com/engstk/android_device_oneplus_cheeseburger/releases>"));
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
                    
                        if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out #useful-links for all Unbrick Tools for the OnePlus 7-series. Those also include the needed software and a manual"));
                        }
                        else if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out #useful-links for all Unbrick Tools for the OnePlus 6 and 6T. Those also include the needed software and a manual"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out #useful-links for all Unbrick Tools for the OnePlus 5 and 5T. Those also include the needed software and a manual"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out #useful-links for all Unbrick Tools for the OnePlus 3 and 3T. Those also include the needed software and a manual"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out #useful-links for all Unbrick Tools for legacy devices. Those also include the needed software and a manual"));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please perform your request in the correct device channel."));
                        }
                        break;
                    case "root":
                    case "magisk":
                        if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Boot temporarily into TWRP using `fastboot boot twrp.img` and select option `Install recovery to ramdisk`. Reboot your phone, come back to TWRP installed in ramdisk and then flash Magisk zip."));
                        }    
                        else if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Boot temporarily into TWRP using `fastboot boot twrp.img` and select option `Install recovery to ramdisk`. Reboot your phone, come back to TWRP installed in ramdisk and then flash Magisk zip."));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Flash magisk zip after installing TWRP (`fastboot flash recovery twrp.img` and `fastboot reboot`) "));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Flash magisk zip after installing TWRP (`fastboot flash recovery twrp.img` and `fastboot reboot`) "));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Flash magisk zip after installing TWRP (`fastboot flash recovery twrp.img` and `fastboot reboot`) "));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please refer to your device channel for detailed instructions."));
                        }
                        break;
                    case "gcam":
                    case "googlecam":
                    case "googlecamera":
                        if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Google Camera for OnePlus 6 & 6T: <https://forum.xda-developers.com/oneplus-6t/themes/app-oneplus-6-google-camera-port-t3862849>" + Environment.NewLine +
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
                        if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("OnePlus 7 Pro mirrors for official OxygenOS: <https://forums.oneplus.com/threads/oneplus-7-pro-5g-rom-ota-oxygen-os-repo-of-oxygen-os-builds.1033129/>"));
                        }
                        else if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("OnePlus 6 mirrors for official OxygenOS: <https://forums.oneplus.com/threads/oneplus-6-rom-ota-oxygen-os-mirrors-for-official-oxygen-os-roms-and-ota-updates.835607>" + Environment.NewLine +
                                             "OnePlus 6T mirrors unavailable. Please use <https://www.oneplus.com/support/softwareupgrade/details?code=9>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("OnePlus 5 mirrors for official OxygenOS: <https://forums.oneplus.com/threads/oneplus-5-rom-ota-oxygen-os-mirrors-for-official-oxygen-os-roms-and-ota-updates.556580>" + Environment.NewLine +
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
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please refer to your device channel"));
                        }
                        break;
                    case "edl":
                    case "9008":
                    case "9008mode":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("To enter in Qualcomm EDL mode, you can use `adb reboot edl` or use any version of blu_spark TWRP based on TWRP 3.3.0 or later by clicking on `Reboot to EDL`." + Environment.NewLine +
                                             "If you want to exit EDL mode, maintain power button during at least 10 seconds " + Environment.NewLine +
                                             "You can use a generic USB-C cable however, OnePlus official cable are preferable"));
                        }
                        break;
                    case "whiteled":
                    case "qualcommdiagnostics":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("If you have a persistent white LED while trying to boot your phone and a black screen, it means that your device is stuck in Qualcomm Diagnostics mode." + Environment.NewLine +
                                             "You can turn off your phone by maintaining power button during at least 10 seconds and then use MSM tool to recover it to a working state" + Environment.NewLine +
                                             "To use MSM tool, you will need to enter in EDL mode. Information on how to do so can be obtained by sending the command `;faq edl`"));
                        }
                        break;
                    case "smt":
                    case "smtdownloadmode":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("If you're reading this, stop what you are doing. Please." + Environment.NewLine +
                                             "The reason why aftersales support of OnePlus use Upgrade Mode in MSM tool during remote assistance sessions rather than SMT mode is that it is meant for factory only as it wipes NV (non volatile) items such as IMEI (source <https://forum.xda-developers.com/showpost.php?p=77937552&postcount=90>)"));
                        }
                        break;
                    case "qpreview":
                    case "qbeta":
                    case "qdevpreview":
                    case "androidqpreview":
                        if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Android Q Dev Preview is now available to download for OnePlus 6 and 6T (source <https://forums.oneplus.com/threads/android-q-beta-developer-preview-for-oneplus-6-6t.1020398/> )" + Environment.NewLine +
                                             "Direct download link for OnePlus 6: <https://oxygenos.oneplus.net/OnePlus6Oxygen_22_OTA_001_all_1905032150_wipe_2fa26de80dec40fb.zip>" + Environment.NewLine +
                                             "Direct download link for OnePlus 6T: <https://oxygenos.oneplus.net/OnePlus6TOxygen_34_OTA_001_all_1905032146_wipe_221b43d22d9b4dd4.zip>" + Environment.NewLine +
                                             "Place the relevant zip at the root of the system storage of your phone and use local update feature (Settings --> System --> System Updates --> click on the wheel) to update to Android Q dev preview." + Environment.NewLine +
                                             "If you want to rollback to stable release of Android Pie, use again local update feature with these zips <https://oxygenos.oneplus.net/fulldowngrade_wipe_MSM_17819_181025_2315_user_MP1_release.zip> (for OP6) / https://oxygenos.oneplus.net/Fulldowngrade_wipe_18801_181024_2027_user_MP2_release.zip (for OP6T)" + Environment.NewLine +
                                              Environment.NewLine +
                                             "**WARNING**: T-Mobile OP6T converted users can flash Q Preview but will have to use MSM tool to recover their device if they wish to downgrade as the rollback zip provided by OnePlus will lead to a device mismatch according to reports on XDA forums (sources <https://forum.xda-developers.com/showpost.php?p=79482702&postcount=2295> and <https://forum.xda-developers.com/showpost.php?p=79483602&postcount=57> )"));
                        }
                        break;
                    case "softwaremaintenanceschedule":
                    case "updateschedule":
                    case "updatewhen":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Oneplus software maintenance schedule is described at <https://forums.oneplus.com/threads/oneplus-software-maintenance-schedule.862347/>"));
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithImageUrl("https://forums.oneplus.com/attachments/806308"));
                        }
                        break;
                    case "adb":
                    case "fastboot":
                    case "adbfastboot":
                    case "adbpath":
                    case "fastbootpath":
                    case "adbfastbootpath":
                    case "platformtools":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("To use ADB and fastboot tools, download them from <https://dl.google.com/android/repository/platform-tools-latest-windows.zip> (source <https://developer.android.com/studio/releases/platform-tools> ) and unzip them. If you wish to add them to your PATH (meaning being able to use them without having to navigate to the folder they're stored first), search in Start Menu for `Environment variables` and click on `Modify system environment variables`. On the next window click on `Environment variables`. Go to `System variables`, select `Path` and click on `Edit`. Click on `New` and input the location of where you unzipped `platform-tools` folder."));
                        }
                        break;
                    default:
                        await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Supported FAQ commands are: bluspark, googlecamera, oxygenos, unbrick, edl, magisk, root, qualcommdiagnostics, smt, qpreview, updateschedule, adbfastbootpath"));
                        break;
                }
            }
            catch (Exception ex)
            {
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription(ex.Message));
            }
        }

        private object EmbedImage()
        {
            throw new NotImplementedException();
        }
    }
}
