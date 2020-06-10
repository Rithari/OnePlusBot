using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using OnePlusBot.Base.Errors;

namespace OnePlusBot.Data.Models
{
    [Table("PersistentData")]
    public class PersistentData
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("name")]
        public string Name { get; set; }
        
        [Column("val")]
        public ulong Value { get; set; }

        [Column("string")]
        public string StringValue { get; set; }

        public static ulong GetConfiguredInt(string name)
        {
          using(var db = new Database())
          {
            return GetConfiguredInt(name, db);
          }
        }

        public static ulong GetConfiguredInt(string name, Database db)
        {
          var value = db.PersistentData.AsQueryable().Where(p => p.Name == name);
          if(value.Any())
          {
            return value.First().Value;
          }
          else
          {
            throw new NotFoundException("Persistent value with name not found");
          }
        }

        public static string GetConfiguredString(string name)
        {
          using(var db = new Database())
          {
            return GetConfiguredString(name, db);
          }
        }

        public static string GetConfiguredString(string name, Database db)
        {
          var value = db.PersistentData.AsQueryable().Where(p => p.Name == name);
          if(value.Any())
          {
            return value.First().StringValue;
          }
          else
          {
            throw new NotFoundException("Persistent value with name not found");
          }
        }
    }
}