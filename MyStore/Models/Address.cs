namespace MyStore.Models
{
    public class Address : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
