using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace OnePlusBot.Data.Models
{
    [Table("Commands")]
    public class Command
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("name")]
        public string Name { get; set; }

        [Column("module_id")]
        public uint ModuleId { get; set; }


        [ForeignKey("ModuleId")]
        public virtual CommandModule Module { get; set; }

        public virtual ICollection<CommandInChannelGroup> GroupsCommandIsIn { get; set; }


        /// <summary>
        /// Returns wheter or not the command should be considered enabled.
        /// A command is enabled if:
        /// its not configured in any channel group of type COMMANDS
        /// its configured in *any* channel group of type COMMANDS, but its enabled in this group
        /// its configured for groups of type COMMANDS, but these groups are disabled
        /// </summary>
        /// <param name="channelId">The id of the channel to check for</param>
        /// <returns>bool wheter or not this command is enabled</returns>
        public bool CommandEnabled(ulong channelId)
        {
            bool result = false;
            var cmdGroups = this.GroupsCommandIsIn.Where(grp => grp.ChannelGroupReference.ChannelGroupType == ChannelGroupType.COMMANDS
                                                          && !grp.ChannelGroupReference.Disabled
                                                          && grp.ChannelGroupReference.Channels.Where(ch => ch.ChannelId == channelId).Any());
            if(cmdGroups.Any())
            {
              result = this.GroupsCommandIsIn.Where(grp => !grp.Disabled).Any();
            }
            else 
            {
              result = true;
            }
          return result;
        }
    }
}