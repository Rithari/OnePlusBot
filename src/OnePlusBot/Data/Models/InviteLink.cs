using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace OnePlusBot.Data.Models
{
    [Table("InviteLinks")]
    public class InviteLink
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }

        [Column("link")]
        public string Link { get; set; }
        
        [Column("label")]
        public string Label { get; set; }
        
        [Column("added_date")]
        public DateTime AddedDate { get; set; }

    }
}