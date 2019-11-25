using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("ExperienceLevels")]
    public class ExperienceLevel
    {
        [Column("level")]
        [Key]
        public uint Level { get; set; }
        
        [Column("needed_experience")]
        public ulong NeededExperience { get; set; }
      
    }
}