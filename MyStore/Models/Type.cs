namespace MyStore.Models
{
    public class Type
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Product> Products { get;}

    }
}
