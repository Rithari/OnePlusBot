using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace OnePlusBot.Data.Models
{
    // not a proper templating engine, just simple string replacements, should be replaced with a proper template engine at some point
    [Table("ResponseTemplate")]
    public class ResponseTemplate
    {
        [Key]
        [Column("template_key")]
        public string Key { get; set; }
        
        [Column("template_text")]
        public string TemplateText { get; set; }

        public static string ILLEGAL_NAME_RESPONSE = "ILLEGAL_NAME_MODMAIL";
        public static string ILLEGAL_NAME_REMINDER = "ILLEGAL_NAME_REMINDER_TEXT";
    }
}