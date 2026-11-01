namespace loginservice.Models
{
    public class Users
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationToken { get; set; }
  
}
}
