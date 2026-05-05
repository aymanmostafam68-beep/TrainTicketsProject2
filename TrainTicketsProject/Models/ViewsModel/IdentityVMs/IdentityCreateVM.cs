namespace TrainTicketsProject.Models.ViewsModel.IdentityVMs
{
    public class IdentityCreateVM
    {
        [Required]
        [Display(Description = "First Name")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "First Name must be between 4 and 15 characters.")]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [Display(Description = "Last Name")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Last Name must be between 4 and 15 characters.")]

        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]

        public string Email { get; set; } = string.Empty;


        public string? Address { get; set; } = string.Empty;
        [Required]
         [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone Number must be 11 characters.")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]

        [StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 characters.")]

        public string NationalId { get; set; } = string.Empty;
        [Required]
        [StringLength(12, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 12 characters.")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare("Password", ErrorMessage = "Passwords does not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string ReturnURL { get; set; } = string.Empty;

    }
}
