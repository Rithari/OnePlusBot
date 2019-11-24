using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("Roles")]
    public class Role
    {
        
        [Column("name")]
        public string Name { get; set; }
        
        [Column("role_id")]
        [Key]
        public ulong RoleID { get; set; }

        [Column("xp_role")]
        public bool XPRole {get; set; }
    }
}