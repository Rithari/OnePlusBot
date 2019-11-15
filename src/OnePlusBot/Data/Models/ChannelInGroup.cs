using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("ChannelInGroup")]
    public class ChannelInGroup
    {
        [Column("channel_id")]
        public ulong ChannelId { get; set; }
        
        [Column("channel_group_id")]
        public uint ChannelGroupId { get; set; }

        [ForeignKey("ChannelGroupId")]
        public virtual ChannelGroup Group { get; set; }

        [ForeignKey("ChannelId")]
        public virtual Channel ChannelReference { get; set; }
      
    }
}