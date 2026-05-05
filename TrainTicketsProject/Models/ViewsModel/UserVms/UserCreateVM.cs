using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrainTicketsProject.Models.ViewsModel.UserVms
{
    public class UserCreateVM
    {
        public string id { get; set; } = string.Empty;

        public string FirstName { get; set; }
        public string LastName { get; set; }


        public string userName { get; set; }
        public string Email { get; set; }
        public bool IsConfirmed { get; set; }

        public string Password { get; set; }
        public string RoleName { get; set; }

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}
