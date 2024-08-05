namespace MyStore.Request
{
    public class VerifyResetTokenRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
