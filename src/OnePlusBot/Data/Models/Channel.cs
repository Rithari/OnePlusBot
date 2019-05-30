using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace OnePlusBot.Data.Models
{
    [Table("Channels")]
    public class Channel
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Obsolete("Are you sure you don't want to use ChannelID?")]
        public uint ID { get; set; }
        
        [Column("name")]
        public string Name { get; set; }

        [Column("channel_id")]
        public ulong ChannelID { get; set; }
        
        [Column("channel_type")]
        public ChannelType ChannelType { get; set; }
    }
}