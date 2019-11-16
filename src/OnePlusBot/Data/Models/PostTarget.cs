using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("PostTargets")]
    public class PostTarget
    {        
        [Column("name")]
        [Key]
        public string Name { get; set; }

        [Column("channel_id")]
        public ulong ChannelId { get; set; }


        [ForeignKey("ChannelId")]
        public virtual Channel ChannelReference { get; set; }

        public static readonly string OFFTOPIC = "offtopic";
        public static readonly string JOIN_LOG = "joinlog";
        public static readonly string BAN_LOG = "banlog";
        public static readonly string DECAY_LOG = "decaylog";
        public static readonly string DELETE_LOG = "deletelog";
        public static readonly string EDIT_LOG = "editlog";
        public static readonly string MODMAIL_LOG = "modmaillog";
        public static readonly string MODMAIL_NOTIFICATION = "modmailnotification";
        public static readonly string MUTE_LOG = "mutelog";
        public static readonly string NEWS = "news";
        public static readonly string PROFANITY_QUEUE = "profanityqueue";
        public static readonly string STARBOARD = "starboard";
        public static readonly string UNBAN_LOG = "unbanlog";
        public static readonly string SUGGESTIONS = "suggestions";
        public static readonly string USERNAME_QUEUE = "usernamequeue";
        public static readonly string WARN_LOG = "warnlog";
        public static readonly string LEAVE_LOG = "leavelog";
      
    }
}