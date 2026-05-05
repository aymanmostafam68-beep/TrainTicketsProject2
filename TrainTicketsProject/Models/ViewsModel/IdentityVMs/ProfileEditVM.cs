namespace TrainTicketsProject.Models.ViewsModel.IdentityVMs
{
    public class ProfileEditVM
    {
        public string Id { get; set; } = "0";
        [Required]
        [Display(Description = "First Name")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "First Name must be between 4 and 15 characters.")]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [Display(Description = "Last Name")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Last Name must be between 4 and 15 characters.")]
        public string LastName { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "Invalid email format")]

        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        [StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 characters.")]

        public string NationalId { get; set; } = string.Empty;
        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone Number must be 11 characters.")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;



    }
}
