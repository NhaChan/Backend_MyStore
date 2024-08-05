using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }
        public string Name { get; set; }
        public string Represent { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
    }
}
