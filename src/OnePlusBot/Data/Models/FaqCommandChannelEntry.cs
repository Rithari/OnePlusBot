
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("FAQCommandChannelEntry")]
    public class FAQCommandChannelEntry
    {
       

        [Key]
        [Column("entry_id", Order=0)]
        public uint EntryId { get; set; }
        
        [Column("text")]
        public string Text { get; set; }

        [Column("is_embed")]
        public bool IsEmbed { get; set; }

        [Column("image_url")]
        public string ImageURL { get; set; }

        [Column("hex_color")]
        public uint HexColor { get; set; }

        [Column("position")]
        public uint Position { get; set; }

        [Column("command_channel_id")]
        public uint CommandChannelId { get; set; }

        [Column("author")]
        public string Author { get; set; }

        [Column("author_avatar_url")]
        public string AuthorAvatarUrl { get; set; }


        [ForeignKey("CommandChannelId")]
        public virtual FAQCommandChannel FAQCommandChannel { get; set; }

    
        // TODO should not be necessary, there is entity state detach, did not work for me
        public FAQCommandChannelEntry clone(){
            var clone = new FAQCommandChannelEntry();
            clone.Author = Author;
            clone.AuthorAvatarUrl = AuthorAvatarUrl;
            clone.Position = Position;
            clone.HexColor = HexColor;
            clone.ImageURL = ImageURL;
            clone.IsEmbed = IsEmbed;
            clone.Text = Text;
            return clone;
        }


    }
}