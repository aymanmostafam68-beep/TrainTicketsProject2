using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrainTicketsProject.Models.ViewsModel.UserVms
{
    public class UserCreateVM
    {
        public string id { get; set; } = string.Empty;
        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "First Name must be between 4 and 15 characters.")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "userName must be between 4 and 15 characters.")]


        public string userName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        public bool IsConfirmed { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 6, ErrorMessage = "Password must be between 8 and 12 characters.")]
        public string Password { get; set; }
        [Required]
        public string RoleName { get; set; }

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}
