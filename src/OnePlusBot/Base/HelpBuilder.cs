using System.ComponentModel;
using System.Linq;
using System.Security.AccessControl;
using System.Data.Common;
using System.Text;
using System;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace OnePlusBot.Base
{
    public class HelpBuilder
    {
      public static string BuildGeneralModuleDescription(ModuleInfo module) 
      {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Module name: **{module.Name}**" + Environment.NewLine);
        sb.Append($"Description: {module.Summary}" + Environment.NewLine);

        return sb.ToString();
      }

      public static string BuildDetailedModuleDescription(ModuleInfo module, SocketCommandContext Context, IServiceProvider services) 
      {
        StringBuilder sb = new StringBuilder();
        sb.Append(HelpBuilder.BuildGeneralModuleDescription(module) + Environment.NewLine);
        sb.Append("Commands: " + Environment.NewLine);
        StringBuilder commandSb = new StringBuilder();
        foreach (var command in module.Commands) 
        {
          var result = command.CheckPreconditionsAsync(Context, services).GetAwaiter().GetResult();
          if(result.IsSuccess) {
            commandSb.Append($"`{command.Name}`");
            // does not play well with the command filtering, edge case
            if(command != module.Commands.Last()) 
            {
              commandSb.Append(", ");
            }
          }
        }
        if(commandSb.ToString() == String.Empty) 
        {
          commandSb.Append("You are not able to execute any command of this module.");
        }
        sb.Append(commandSb.ToString());
        return sb.ToString();
      }

      public static string BuildDetailedCommandDescription(CommandInfo command) 
      {
        StringBuilder sb = new StringBuilder();
          StringBuilder preconditions = new StringBuilder("");
          foreach(var pre in command.Preconditions){
            if(pre is RequireRole)
            {
              RequireRole casted = pre as RequireRole;
              preconditions.Append("Required role: ");
              if(casted.AllowedRoles.Length > 1)
              {
                string roleConcatenation = casted.mode == ConcatenationMode.AND ? " AND " : " OR ";
                preconditions.Append(string.Join(roleConcatenation, casted.AllowedRoles));
              }
              else
              {
                preconditions.Append(casted.AllowedRoles[0]);
              }
              
            } 
          }
          if(preconditions.ToString() != string.Empty)
          {
            preconditions.Append("\n");
          }
          sb.Append($"Command name: **{command.Name}**" + Environment.NewLine);
          sb.Append($"Description: {command.Summary}" + Environment.NewLine);
          sb.Append(preconditions.ToString());
          sb.Append( (command.Aliases.Any() ? $"**Aliases:** {string.Join(", ", command.Aliases.Select(x => $"`{x}`"))}\n" : ""));
          sb.Append($"**Usage:** `{HelpBuilder.GetPrefix(command)} {GetAliases(command)}`");
          return sb.ToString();
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

        private static string GetAliases(CommandInfo command)
        {
            if (!command.Parameters.Any()) 
                return string.Empty;
            
            var output = new StringBuilder();
            foreach (var param in command.Parameters)
            {
                if (param.IsOptional) {
                  output.AppendFormat("[{0} = {1}] ", param.Name, param.DefaultValue);
                }
                else if (param.IsMultiple) {
                    output.AppendFormat("|{0}| ", param.Name);
                }
                else if (param.IsRemainder) {
                    output.AppendFormat("...{0} ", param.Name); 
                    } 
                else {
                    output.AppendFormat("<{0}> ", param.Name);
                }
            }
            return output.ToString();
        }

      
    }
}