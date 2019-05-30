using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Obsolete("Are you sure you don't want to use RoleID?")]
        public uint ID { get; set; }
        
        [Column("name")]
        public string Name { get; set; }
        
        [Column("role_id")]
        public ulong RoleID { get; set; }
    }
}