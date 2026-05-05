namespace TrainTicketsProject.Models.ViewsModel.IdentityVMs
{
    public class IdentityVM
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;


        [Required]
        public string Password { get; set; } = string.Empty;

        public string ReturnURL { get; set; } = string.Empty;
    }
}
