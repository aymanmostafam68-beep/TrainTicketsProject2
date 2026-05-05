namespace TrainTicketsProject.Models.ViewsModel.IdentityVMs
{
    public class OTPCodeVM
    {
        public string Id { get; set; }
        [Required]
        public int OTP { get; set; }
    }
}
