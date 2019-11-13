using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("ChannelGroups")]
    public class ChannelGroup
    {
        [Column("id")]
        [Key]
        public uint Id { get; set; }
        
        [Column("name")]
        public string Name { get; set; }


        public virtual ICollection<ChannelInGroup> Channels { get; set; }
      
    }
}