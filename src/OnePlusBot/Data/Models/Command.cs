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


        public bool CommandEnabled(ulong channelId)
        {
            return this.GroupsCommandIsIn.Where(grp => !grp.Disabled && grp.ChannelGroupReference.Channels.Where(ch => ch.ChannelId == channelId).Any()).Any();
        }
    }
}