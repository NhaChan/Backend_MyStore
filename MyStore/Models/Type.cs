namespace MyStore.Models
{
    public class Type : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Product> Products { get;}

    }
}
