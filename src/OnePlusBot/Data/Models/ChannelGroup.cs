using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Column("profanity_check_exempt")]
        public bool ProfanityCheckExempt { get; set;}

        [Column("invite_check_exempt")]
        public bool InviteCheckExempt { get; set;}

        [Column("exp_gain_exempt")]
        public bool ExperienceGainExempt { get; set; }

        [Column("disabled")]
        public bool Disabled { get; set; }

        [Column("channel_group_type")]
        public string ChannelGroupType { get; set; }

        public virtual ICollection<ChannelInGroup> Channels { get; set; }

        public virtual ICollection<CommandInChannelGroup> Commands { get; set; }     
      
    }

      public static class ChannelGroupType {
          public static string CHECKS = "CHECKS";

          public static string FAQ = "FAQ";
          
          public static string COMMANDS = "COMMANDS";

          public static List<string> TYPES =
                              new List<string> { 
                               CHECKS, FAQ, COMMANDS
                              };
        }
}