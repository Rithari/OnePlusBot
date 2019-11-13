using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("ExperienceRoles")]
    public class ExperienceRole
    {
        [Column("id")]
        [Key]
        public uint Id { get; set; }

        [Column("role_id")]
        public uint ExperienceRoleId { get; set; }
        
        [Column("level")]
        public uint Level { get; set; }
      
        [ForeignKey("ExperienceRoleId")]
        public virtual Role RoleReference { get; set; }

        [ForeignKey("Level")]
        public virtual ExperienceLevel LevelReference { get; set; }

    }
}