using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("CommandInChannelGroup")]
    public class CommandInChannelGroup
    {
        [Column("command_id")]
        public uint CommandID { get; set; }
        
        [Column("channel_group_id")]
        public uint ChannelGroupId { get; set; }

        [Column("disabled")]
        public bool Disabled { get; set; }

        [ForeignKey("CommandID")]
        public virtual Command CommandReference { get; set; }

        [ForeignKey("ChannelGroupId")]
        public virtual ChannelGroup ChannelGroupReference { get; set; }
    }
}