
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using OnePlusBot.Helpers;

namespace OnePlusBot.Data.Models
{
    [Table("FaqCommandChannel")]
    public class FAQCommandChannel : IPaginatable
    {

        [Key]
        [Column("command_channel_id")]
        public uint CommandChannelId { get; set; }

        [Column("command_id")]
        public uint FAQCommandId { get; set; }

        [Column("channel_id")]
        public uint ChannelId { get; set; }


        [ForeignKey("ChannelId")]
        public virtual Channel Channel { get; set; }


        [ForeignKey("FAQCommandId")]
        public virtual FAQCommand Command { get; set; }
    
        public virtual ICollection<FAQCommandChannelEntry> CommandChannelEntries { get; set; }

        public string display(){
            return $"ID: {CommandChannelId}: command: {Command.Name} in {Channel.Name}";
        }
    }
}