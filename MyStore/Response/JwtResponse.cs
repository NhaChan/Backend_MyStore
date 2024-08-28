namespace MyStore.Response
{
    public class JwtResponse
    {
        public string Access_token { get; set; }
        public string Refresh_token { get; set; }
        public string? PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
        public IEnumerable<string> Roles { get; set; } = [];
    }
}
