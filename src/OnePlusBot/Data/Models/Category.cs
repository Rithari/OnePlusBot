using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace OnePlusBot.Data.Models
{
    [Table("Categories")]
    public class Category
    {
       
        [Column("name")]
        public string Name { get; set; }

        [Column("id")]
        [Key]
        public ulong ID { get; set; }
        
        [Column("log_disabled")]
        public Boolean LogDisabled { get; set; }

    }
}