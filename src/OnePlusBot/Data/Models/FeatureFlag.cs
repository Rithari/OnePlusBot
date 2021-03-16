using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("FeatureFlags")]
    public class FeatureFlag
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("feature_name")]
        public String FeatureName { get; set; }

        [Column("enabled")]
        public Boolean Enabled { get; set; }

        public static String STARBOARD = "starboard";
        public static String LINK_EMBEDS = "linkEmbeds";
        public static String EXPERIENCE = "experience";
        public static String MODERATION = "moderation";
        public static String MODMAIL = "modmail";
        public static String PROFANITY = "profanity";

        // join leave listener for logging
        public static String JOIN_LEAVE = "joinLeave";

        public static String EMOTE_TRACKING = "emoteTracking";

        public static String MASS_PING = "massPing";

        public static String LOGGING = "logging";

        public static String CONTEXT_VOICE_CHANNEL = "voiceChannel";

        public static String REFERRAL = "referral";

        public static String SETUPS = "setups";

        public static String INVITE_CHECK = "inviteCheck";

        public static String ASSIGNABLE_ROLES = "assignableRole";

        public static String REMINDER = "reminder";
    }
}