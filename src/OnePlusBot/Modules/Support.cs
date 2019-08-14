using Discord;
using Discord.Commands;
using OnePlusBot.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class Support : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public Support(IServiceProvider services, CommandService commands)
        {
            _commands = commands;
            _services = services;
        }

        [
            Command("help"),
            Summary("Lists all available commands.")
        ]
        public async Task Help(string path = "")
        {
            int r = Global.Random.Next(256);
            int g = Global.Random.Next(256);
            int b = Global.Random.Next(256);


            var output = new EmbedBuilder();
            if (path == string.Empty)
            {
                output.Title = "OnePlusBot - help";
                output.WithColor(r, g, b);

                foreach (var mod in _commands.Modules.Where(m => m.Parent == null))
                {
                    AddHelp(mod, ref output);
                }

                output.WithFooter("Use 'help <module>' to get help with a module.");
            }
            else
            {
                var mod = _commands.Modules.FirstOrDefault(m => string.Equals(m.Name.Replace("Module", ""), path, StringComparison.OrdinalIgnoreCase));
                if (mod == null)
                {
                    await ReplyAsync("No module could be found with that name."); 
                    return;
                }

                output.Title = mod.Name;
                output.Description = $"{mod.Summary}\n" +
                    (!string.IsNullOrEmpty(mod.Remarks) ? $"({mod.Remarks})\n" : "") +
                    (mod.Aliases.Any() ? $"Prefix(es): {string.Join(",", mod.Aliases)}\n" : "") +
                    (mod.Submodules.Any() ? $"Submodules: {mod.Submodules.Select(m => m.Name)}\n" : "") + " ";
                AddCommands(mod, ref output);
            }

            await ReplyAsync("", embed: output.Build());
        }

        public void AddHelp(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var sub in module.Submodules) AddHelp(sub, ref builder);
            builder.AddField(f =>
            {
                f.Name = $"**{module.Name}**";
                f.Value = $"Submodules: {string.Join(", ", module.Submodules.Select(m => m.Name))}" +
                $"\n" +
                $"Commands: {string.Join(", ", module.Commands.Select(x => $"`{x.Name}`"))}";
            });
        }

        public void AddCommands(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var command in module.Commands)
            {
                command.CheckPreconditionsAsync(Context, _services).GetAwaiter().GetResult();
                AddCommand(command, ref builder);
            }

        }

        public void AddCommand(CommandInfo command, ref EmbedBuilder builder)
        {
            builder.AddField(f =>
            {
                f.Name = $"**{command.Name}**";
                f.Value = $"{command.Summary}\n" +
                (!string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})\n" : "") +
                (command.Aliases.Any() ? $"**Aliases:** {string.Join(", ", command.Aliases.Select(x => $"`{x}`"))}\n" : "") +
                $"**Usage:** `{GetPrefix(command)} {GetAliases(command)}`";
            });
        }

        private static string GetAliases(CommandInfo command)
        {
            if (!command.Parameters.Any()) 
                return string.Empty;
            
            var output = new StringBuilder();
            foreach (var param in command.Parameters)
            {
                if (param.IsOptional)
                    output.AppendFormat("[{0} = {1}] ", param.Name, param.DefaultValue);
                else if (param.IsMultiple)
                    output.AppendFormat("|{0}| ", param.Name);
                else if (param.IsRemainder)
                    output.AppendFormat("...{0} ", param.Name);
                else
                    output.AppendFormat("<{0}> ", param.Name);
            }
            return output.ToString();
        }

        private static string GetPrefix(CommandInfo command)
        {
            var output = GetPrefix(command.Module);
            output += $"{command.Aliases.FirstOrDefault()} ";
            return output;
        }

        private static string GetPrefix(ModuleInfo module)
        {
            string output = "";
            if (module.Parent != null) 
                output = $"{GetPrefix(module.Parent)}";
            
            if (module.Aliases.Any())
                output += string.Concat(module.Aliases.FirstOrDefault(), " ");
            return output;
        }

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
                    
                        if (Context.Channel.Name == "general")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for all OnePlus-devices: <https://www.oneplus.com/support/repair-pricing>"));
                        }
                        else if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus 7: <https://www.oneplus.com/support/repair-pricing/details?code=10>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 7 Pro: <https://www.oneplus.com/support/repair-pricing/details?code=11>"));
                        }
                        else if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus 6: <https://www.oneplus.com/support/repair-pricing/details?code=8>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 6T: <https://www.oneplus.com/support/repair-pricing/details?code=9>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus 5: <https://www.oneplus.com/support/repair-pricing/details?code=5>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 5T: <https://www.oneplus.com/support/repair-pricing/details?code=7>"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus 3: <https://www.oneplus.com/support/repair-pricing/details?code=4>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 3T: <https://www.oneplus.com/support/repair-pricing/details?code=6>"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Repair prices for the OnePlus One: <https://www.oneplus.com/support/repair-pricing/details?code=1>" + Environment.NewLine +
                                             "Repair prices for the OnePlus 2: <https://www.oneplus.com/support/repair-pricing/details?code=2>" + Environment.NewLine +
                                             "Repair prices for the OnePlus X: <https://www.oneplus.com/support/repair-pricing/details?code=3>"));
                        }
                        break;
                    case "blu_spark":
                    case "bluspark":
                        if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus 7 Pro: <https://forum.xda-developers.com/oneplus-7-pro/development/kernel-t3944179/>" + Environment.NewLine +
                                             "blu_spark XDA thread for OnePlus 7: <https://forum.xda-developers.com/oneplus-7/development/kernel-t3944855>"));
                        }
                        else if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus 6: <https://forum.xda-developers.com/oneplus-6/development/kernel-t3800965>" + Environment.NewLine +
                                             "blu_spark XDA thread for OnePlus 6T: <https://forum.xda-developers.com/oneplus-6t/development/kernel-t3861123>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus 5: <https://forum.xda-developers.com/oneplus-5/development/kernel-t3651933>" + Environment.NewLine +
                                             "blu_spark XDA thread for OnePlus 5T: <https://forum.xda-developers.com/oneplus-5t/development/kernel-t3706295>"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus 3 and 3T: <https://forum.xda-developers.com/oneplus-3/oneplus-3--3t-cross-device-development/kernel-t3404970>"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark XDA thread for OnePlus X: <https://forum.xda-developers.com/oneplus-x/orig-development/kernel-t3250995>"));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please perform your request in a device channel."));
                        }
                        break;
                    case "twrp":
                        if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark TWRP download link for OnePlus 7 and 7 Pro: <https://github.com/engstk/android_device_oneplus_guacamole_unified_TWRP/releases>"));
                        }
                        else if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark TWRP download link for OnePlus 6: <https://github.com/engstk/android_device_oneplus_enchilada/releases>" + Environment.NewLine +
                                             "blu_spark TWRP Download link for OnePlus 6T: <https://github.com/engstk/android_device_oneplus_fajita/releases>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("blu_spark TWRP download link for OnePlus 5 and 5T: <https://github.com/engstk/android_device_oneplus_cheeseburger/releases>"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Official TWRP download link for OnePlus 3 and 3T: <https://twrp.me/oneplus/oneplusthree.html>"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Official TWRP download link for OnePlus X: <https://twrp.me/oneplus/oneplusx.html>" + Environment.NewLine +
                                             "Official TWRP download link for OnePlus 2: <https://twrp.me/oneplus/oneplustwo.html>" + Environment.NewLine +
                                             "Official TWRP download link for OnePlus One: <https://twrp.me/oneplus/oneplusone.html>"));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please perform your request in a device channel."));
                        }
                        break;
                    case "unbrick":
                    case "bricked":
                    case "brick":
                    case "unbricktool":
                    
                        if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out <#451373145010470912> for all Unbrick Tools for the OnePlus 7-series. Those also include the needed software and a manual")); // 451373145010470912 is channel ID of #useful-links in /r/oneplus server
                        }
                        else if (Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out <#451373145010470912> for all Unbrick Tools for the OnePlus 6 and 6T. Those also include the needed software and a manual"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out <#451373145010470912> for all Unbrick Tools for the OnePlus 5 and 5T. Those also include the needed software and a manual"));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out <#451373145010470912> for all Unbrick Tools for the OnePlus 3 and 3T. Those also include the needed software and a manual"));
                        }
                        else if (Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Check out <#451373145010470912> for all Unbrick Tools for legacy devices. Those also include the needed software and a manual"));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please perform your request in the correct device channel."));
                        }
                        break;
                    case "root":
                    case "magisk":
                        if (Context.Channel.Name == "oneplus7-series" || Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Boot temporarily into TWRP using `fastboot boot twrp.img` and select option `Install recovery to ramdisk`. Reboot your phone, come back to TWRP installed in ramdisk and then flash Magisk zip." + Environment.NewLine +
                                             "As a reminder, Magisk does not have a website and the only place to get it is <https://github.com/topjohnwu/Magisk/releases>"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t" || Context.Channel.Name == "oneplus3-3t" || Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Unlock bootloader using `fastboot oem unlock`. Flash magisk zip after installing TWRP (`fastboot flash recovery twrp.img` and `fastboot reboot`) " + Environment.NewLine +
                                             "As a reminder, Magisk does not have a website and the only place to get it is <https://github.com/topjohnwu/Magisk/releases>"));
                        }
                        else
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Please refer to your device channel for detailed instructions."));
                        }
                        break;
                    case "gcam":
                    case "googlecam":
                    case "googlecamera":
                        if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Google Camera for OnePlus 7 & 7 Pro: <https://forum.xda-developers.com/oneplus-7-pro/themes/google-camera-op7-pro-t3944422>" + Environment.NewLine +
                                             "For all device APKs, please visit: <https://www.celsoazevedo.com/files/android/google-camera>"));
                        }
                        else if (Context.Channel.Name == "oneplus6-6t")
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
                        if (Context.Channel.Name == "oneplus7-series" || Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("To enter in Qualcomm EDL mode, you can use `adb reboot edl` or use any version of blu_spark TWRP based on TWRP 3.3.0 or later by clicking on `Reboot to EDL`. An alternative way is using `reboot edl` command in a rooted terminal emulator on your device. You can also power off your device, wait 10 seconds and maintain volume up and down keys." + Environment.NewLine +
                                             "If you want to exit EDL mode, maintain power button during at least 10 seconds " + Environment.NewLine +
                                             "You can use a generic USB-C cable however, OnePlus official cable are preferable"));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t" || Context.Channel.Name == "oneplus3-3t" || Context.Channel.Name == "legacy")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("To enter in Qualcomm EDL mode, you can use `adb reboot edl` or use any version of blu_spark TWRP based on TWRP 3.3.0 or later by clicking on `Reboot to EDL`. You can also power off your device, wait 10 seconds and maintain volume up keys" + Environment.NewLine +
                                             "If you want to exit EDL mode, maintain power button during at least 10 seconds " + Environment.NewLine +
                                             "You can use a generic USB-C cable however, OnePlus official cable are preferable"));
                        }
                        break;
                    case "sahara":
                    case "saharaerror":
                    case "saharacommunication":
                    case "saharacommunicationfailed":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("If when using msm tool you have the error `Sahara communication failed`, follow this procedure to get rid of it." + Environment.NewLine +
                                             "Press Stop button in MSM tool interface" + Environment.NewLine +
                                             "Unplug your phone, maintain power button and any volume button to get it out of EDL mode until you feel a vibration." + Environment.NewLine +
                                             "Wait 15 seconds." + Environment.NewLine +
                                             "Maintain power button for 10 seconds." + Environment.NewLine +
                                             "Get back in EDL mode, plug your phone to your computer, click on Enum button and click on Start button"));
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
                        if (Context.Channel.Name == "oneplus7-series" || Context.Channel.Name == "oneplus6-6t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("If you're reading this, stop what you are doing. Please." + Environment.NewLine +
                                             "The reason why aftersales support of OnePlus use Upgrade Mode in MSM tool during remote assistance sessions rather than SMT mode is that it is meant for factory only as it wipes NV (non volatile) items such as IMEI (source <https://forum.xda-developers.com/showpost.php?p=77937552&postcount=90>)" + Environment.NewLine +
                                             "You will also lose your Widevine L1 certificate."));
                        }
                        else if (Context.Channel.Name == "oneplus5-5t")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("If you're reading this, stop what you are doing. Please." + Environment.NewLine +
                                             "The reason why aftersales support of OnePlus use Upgrade Mode in MSM tool during remote assistance sessions rather than SMT mode is that it is meant for factory only as it wipes NV (non volatile) items such as IMEI (source <https://forum.xda-developers.com/showpost.php?p=77937552&postcount=90>)." + Environment.NewLine +
                                             "You will also lose your Widevine L1 certificate (if your device is one of the units released before march 2018 with Widevine level being L3 and that you bothered sending your device back to OnePlus to get it updated to L1). Devices manufactured after March 2018 should have L1 certificate out of the box."));
                        }
                        else if (Context.Channel.Name == "oneplus3-3t" || Context.Channel.Name == "legacy")
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
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Android Q Dev Preview is now available to download for OnePlus 6 and 6T (source <https://forums.oneplus.com/threads/android-q-developer-preview-3-for-oneplus-6-and-6t.1076551/> )" + Environment.NewLine +
                                             "Direct download link for OnePlus 6: <https://oxygenos.oneplus.net/OnePlus6Oxygen_22_OTA_003_all_1907120020_wipe_ae2a3b38959345b1.zip>" + Environment.NewLine +
                                             "Direct download link for OnePlus 6T: <https://oxygenos.oneplus.net/OnePlus6TOxygen_34_OTA_003_all_1907120039_wipe_d9154a280c58444c.zip>" + Environment.NewLine +
                                             "Place the relevant zip at the root of the system storage of your phone and use local update feature (Settings --> System --> System Updates --> click on the wheel) to update to Android Q dev preview." + Environment.NewLine +
                                             "If you want to rollback to stable release of Android Pie, use again local update feature with these zips <https://oxygenos.oneplus.net/fulldowngrade_wipe_MSM_17819_181025_2315_user_MP1_release.zip> (for OP6) / <https://oxygenos.oneplus.net/Fulldowngrade_wipe_18801_181024_2027_user_MP2_release.zip> (for OP6T)" + Environment.NewLine +
                                              Environment.NewLine +
                                             "**WARNING**: T-Mobile OP6T converted users can flash Q Preview but will have to use MSM tool to recover their device if they wish to downgrade as the rollback zip provided by OnePlus will lead to a device mismatch according to reports on XDA forums (sources <https://forum.xda-developers.com/showpost.php?p=79482702&postcount=2295> and <https://forum.xda-developers.com/showpost.php?p=79483602&postcount=57> )"));
                        }
                        else if (Context.Channel.Name == "oneplus7-series")
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Android Q Dev Preview is now available to download for OnePlus 7 and 7 Pro (source <https://forums.oneplus.com/threads/android-q-developer-preview-4-for-oneplus-7-pro-and-7.1086050/> )" + Environment.NewLine +
                                             "Direct download link for OnePlus 7: <https://oxygenos.oneplus.net/OnePlus7Oxygen_13.X.04_OTA_004_all_1908020007_38a5137d9f554eac.zip>" + Environment.NewLine +
                                             "Direct download link for OnePlus 7 Pro: <https://oxygenos.oneplus.net/OnePlus7ProOxygen_13.X.04_OTA_004_all_1908020003_726196c3b79b4f85.zip>" + Environment.NewLine +
                                             "Place the relevant zip at the root of the system storage of your phone and use local update feature (Settings --> System --> System Updates --> click on the wheel) to update to Android Q dev preview." + Environment.NewLine +
                                             "If you want to rollback to stable release of Android Pie, use again local update feature with these zips <https://oxygenos.oneplus.net/fulldowngrade_wipe_MSM_18857_190505_1527_user.zip> (for OP7) / <https://oxygenos.oneplus.net/Fulldowngrade_wipe_18821_190425_0253_user_fix_revision_MP_release.zip> (for OP 7 Pro)" + Environment.NewLine +
                                              Environment.NewLine +
                                             "**WARNING**: T-Mobile OP 7 Pro converted users can flash Q Preview but will have to use MSM tool to recover their device if they wish to downgrade as the rollback zip provided by OnePlus will lead to a device mismatch.)"));
                        }
                        break;
                    case "softwaremaintenanceschedule":
                    case "updateschedule":
                    case "updatewhen":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("If you want to skip the incremental roll-out from OnePlus, you can use Oxygen Updater to receive updates quicker: https://play.google.com/store/apps/details?id=com.arjanvlek.oxygenupdater. OnePlus software maintenance schedule is described at <https://forums.oneplus.com/threads/oneplus-software-maintenance-schedule.862347/>"));
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
                    case "oneplusswitch":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("You can use OnePlus Switch <https://play.google.com/store/apps/details?id=com.oneplus.backuprestore> to backup and restore your data. Most of applications data will be backed up as well." + Environment.NewLine +
                                             "If you want to avoid data of a specific application to be restored, you can get in `MobileBackup --> App` in `opbackup` folder to delete the .tar file associated to the app (eg: `com.whatsapp.tar` for data of WhatsApp)"));
                        }
                        break;
                    case "readback":
                        {
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("You can use readback function of MSM tool to dump your device partitions. They will be stored at the root of your system drive and have .img as file extension. For that, get in EDL mode, open MSM tool, press F8, tick partitions you want to dump, input `oneplus` as password, press `ok` and click on Readback button" + Environment.NewLine +
                                             "Dumped partitions are flashable by fastboot."));
                        }
                        break;
                    //placeholder for any new FAQ entry that might be added in the future
                    default:
                        await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("Supported FAQ commands are: repairprices, bluspark, googlecamera, oxygenos, unbrick, edl, magisk, root, qualcommdiagnostics, smt, qpreview, updateschedule, adbfastbootpath, saharacommunicationfailed, oneplusswitch, readback" + Environment.NewLine + 
                                         "Mind that some commands can only be used in specific (device)channels"));
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
