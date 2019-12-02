using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("Emotes")]
    public class StoredEmote
    {
       
        [Column("id")]
        [Key]
        public uint ID { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("emote_key")]
        public string Key { get; set; }

        [Column("emote_id")]
        public ulong EmoteId { get; set; }

        [Column("animated")]
        public bool Amimated { get; set; }

        [Column("custom")]
        public bool Custom { get; set; }

        public virtual ICollection<ReactionRole> EmoteReaction { get; set; }

        public IEmote GetAsEmote(){
          if(Custom){
             var animatedPart = this.Amimated ? "a" : "";
            return Emote.Parse($"<{animatedPart}:{Name}:{EmoteId}>");
          }
          else
          {
            return new Emoji(Name);
          }
        }

    }
}