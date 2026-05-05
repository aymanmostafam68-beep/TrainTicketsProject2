namespace TrainTicketsProject.Models.IdentityModel
{
    public class ApplicationUser : IdentityUser
    {
        public string? address { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string NationalId { get; set; } = string.Empty;


        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;

    }
}
