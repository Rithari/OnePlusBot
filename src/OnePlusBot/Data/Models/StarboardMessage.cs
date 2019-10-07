using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("StarboardMessages")]
    public class StarboardMessage
    {        
        [Key]
        [Column("message_id")]
        public ulong MessageId { get; set; }

        [Column("starboard_message_id")]
        public ulong StarboardMessageId { get; set; }

        [Column("star_count")]
        public uint Starcount { get; set; }

        [Column("author_id")]
        public ulong AuthorId { get; set; }

        [Column("ignored")]
        public bool Ignored { get; set; }

    }

}