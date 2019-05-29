using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("Channels")]
    public class Channel
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("name")]
        public string Name { get; set; }

        [Column("channel_id")]
        public ulong ChannelID { get; set; }
    }
}