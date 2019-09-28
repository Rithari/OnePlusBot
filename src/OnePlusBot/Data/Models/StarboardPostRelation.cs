using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("StarboardPostRelations")]
    public class StarboardPostRelation
    {
        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("message_id")]
        public ulong MessageId { get; set; }

        [ForeignKey("MessageId")]
        public virtual StarboardMessage Message { get; set; }
    }
}