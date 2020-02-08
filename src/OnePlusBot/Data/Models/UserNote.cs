using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace OnePlusBot.Data.Models
{
    [Table("UserNote")]
    public class UserNote
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("note_text")]
        public string NoteText { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }
    }
}