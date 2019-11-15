using System.Linq;
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
        public ulong ChannelID { get; set; }
        
        [Column("channel_type")]
        public ChannelType ChannelType { get; set; }

        public virtual ICollection<ChannelInGroup> GroupsChannelIsIn { get; set; }

        public bool ProfanityExempt()
        {
            return this.GroupsChannelIsIn.Where(grp => grp.Group.ProfanityCheckExempt && !grp.Group.Disabled).FirstOrDefault() != null;
        }

        public bool InviteCheckExempt()
        {
            return this.GroupsChannelIsIn.Where(grp => grp.Group.InviteCheckExempt && !grp.Group.Disabled).FirstOrDefault() != null;
        }

        public bool ExperienceGainExempt()
        {
            return this.GroupsChannelIsIn.Where(grp => grp.Group.ExperienceGainExempt && !grp.Group.Disabled).FirstOrDefault() != null;
        }

        public static readonly string STARBOARD = "starboard";
        public static readonly string REFERRAL = "referralcodes";

        public static readonly string SETUPS = "setups";
        public static readonly string INFO = "info";
        

    }
}