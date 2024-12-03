namespace MyStore.DTO
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string? Email { get; set; }
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public IList<string> Roles { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserCreateDTO
    {
        public string Email { get; set; }
        public string? FullName { get; set; }
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class UserUpdateDTO
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public IList<string>? Roles { get; set; }
    }
    public class RoleUserDTO
    {
        public string UserId { get; set; }

        public string Role { get; set; }
    }
}
