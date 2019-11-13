using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("Channels")]
    public class Channel
    {
       
        [Column("name")]
        public string Name { get; set; }

        [Column("channel_id")]
        [Key]
        public ulong ChannelID2 { get; set; }
        
        [Column("channel_type")]
        public ChannelType ChannelType { get; set; }

        [Column("profanity_check_exempt")]
        public bool ProfanityCheckExempt{ get; set;}

        [Column("invite_check_exempt")]
        public bool InviteCheckExempt{ get; set;}

        public virtual ICollection<FAQCommandChannel> CommandChannels { get; set; }

        public static readonly string STARBOARD = "starboard";
        public static readonly string REFERRAL = "referralcodes";

        public static readonly string SETUPS = "setups";
        public static readonly string INFO = "info";
        

    }
}