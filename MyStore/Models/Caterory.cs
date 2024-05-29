using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Caterory
    {
        [Key]
        public long CategoryID { get; set; }
        public string CategoryName { get; set; }

    }
}
