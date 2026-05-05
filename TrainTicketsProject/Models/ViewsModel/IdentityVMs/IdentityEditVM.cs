namespace TrainTicketsProject.Models.ViewsModel.IdentityVMs
{
    public class IdentityEditVM
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        [StringLength(12, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 12 characters.")]
        public string NewPassword { get; set; } = string.Empty;
        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords does not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
