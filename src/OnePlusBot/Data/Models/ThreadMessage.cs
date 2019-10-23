using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("ThreadMessage")]
    public class ThreadMessage
    {
        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        [Column("user_message_id")]
        public ulong UserMessageId { get; set; }


        [Column("channel_message_id")]
        public ulong ChannelMessageId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("anonymous")]
        public bool Anonymous { get; set; }

        [ForeignKey("ChannelId")]
        public virtual ModMailThread Thread { get; set; }
    }
}