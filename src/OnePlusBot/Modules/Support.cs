using Discord;
using Discord.Commands;
using OnePlusBot.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnePlusBot.Helpers;
using Discord.WebSocket;
using System.Runtime.InteropServices;

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
        public async Task FAQAsync([Optional] [Remainder] string parameter)
        {
            var contextChannel = Context.Channel;
            if(parameter == null || parameter == string.Empty)
            {
                await PrintAvailableCommands(contextChannel);
                return;
            }
            var commands = Global.FAQCommands;
            parameter = parameter.Trim();
            var appropriateCommand = commands.Where(m => 
            {
                bool usingAlias = Array.Exists(m.IndividualAliases(), alias => alias.Equals(parameter));
                return (m.Name == parameter || usingAlias);
            });
            if(appropriateCommand.Any())
            {
                var matchingCommand = appropriateCommand.First();
                if(matchingCommand.CommandChannels != null) 
                {
                    var commandChannels = matchingCommand.CommandChannels.Where(cha => cha.Channel.ChannelID == contextChannel.Id);
                    if(commandChannels.Any())
                    {
                        var entries = commandChannels.First().CommandChannelEntries.OrderBy(entry => entry.Position);
                        if(entries.Any())
                        {
                            foreach(var entry in entries)
                            {
                                if(!entry.IsEmbed)
                                {
                                    await Context.Channel.SendMessageAsync(entry.Text);
                                }
                                else 
                                {
                                    var embed = Extensions.FaqCommandEntryToBuilder(entry);
                                    await Context.Channel.SendMessageAsync(embed: embed.Build());
                                }
                            }
                        } 
                        else
                        {
                            await Context.Channel.SendMessageAsync($"Channel has no posts configured for command {appropriateCommand.First().Name}.");
                        }
                    }
                    else
                    {
                        await PrintAvailableCommands(contextChannel);
                    }
                }
                else 
                {
                    await Context.Channel.SendMessageAsync($"Channel has no entry configured for command {appropriateCommand.First().Name}.");
                }
                
            }
            else 
            {
                 await PrintAvailableCommands(contextChannel);
            }
        }
         public async Task PrintAvailableCommands(ISocketMessageChannel contextChannel){
            var commandsAvailable = Global.FAQCommandChannels.Where(ch => ch.Channel.ChannelID == contextChannel.Id).ToList();
            if(commandsAvailable.Count() == 0){
                await Context.Channel.SendMessageAsync("No entry available.");
            } else {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Available entries in this channel " + Environment.NewLine);
                for(var index = 0; index < commandsAvailable.Count; index++)
                {
                    var command = commandsAvailable[index];
                    stringBuilder.Append($"{command.Command.Name}");
                    if(index < commandsAvailable.Count -1 ){
                        stringBuilder.Append(", ");
                    }
                }
                await Context.Channel.SendMessageAsync(stringBuilder.ToString());
            }
           
        }
    }

   
}
