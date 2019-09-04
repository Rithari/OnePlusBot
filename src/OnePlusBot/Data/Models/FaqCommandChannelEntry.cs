
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

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

        [Column("command_channel_id_reference")]
        public uint CommandChannelId { get; set; }

        [Column("author")]
        public string Author { get; set; }

        [Column("author_avatar_url")]
        public string AuthorAvatarUrl { get; set; }


        [Column("changed_date")]
        public DateTime ChangedDate { get; set; }


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
            // I think setting the changed date to now is fine, because we use the cloning, when we use an entry from an already existing channel
            // that means in the process of a creating an entry, and with that we basically say 'this is right'
            clone.ChangedDate = DateTime.Now;
            return clone;
        }
        
        public bool Equals(FAQCommandChannelEntry other){
            if(this == other) return true;
            if(this.HexColor != other.HexColor) return false;
            if(this.Text != other.Text) return false;
            if(this.IsEmbed != other.IsEmbed) return false;
            if(this.Author != other.Author)  return false;
            if(this.AuthorAvatarUrl != other.AuthorAvatarUrl) return false;
            if(this.ImageURL != other.ImageURL) return false;
            return true;
        }

    }
}