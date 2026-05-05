namespace TrainTicketsProject.Models.IdentityModel
{
    public class ApplicationUserOTP
    {
        public int id { get; set; }
        public string OTP { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }  = DateTime.UtcNow;
        public DateTime ExpireAt { get; set; }

        public bool IsUsed { get; set; }
        public bool IsValid { get; set; }


        public string ApplicationUserId { get; set; } = string.Empty;
        public string ApplicationUserName { get; set; } = string.Empty;
    }
}
