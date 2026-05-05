using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrainTicketsProject.Models.ViewsModel.UserVms
{
    public class UsersVM
    {
        public IEnumerable<UserVM> UsersWitRoles { get; set; } = default!;
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();

        public string? SelectedRole { get; set; }
        public string? UserName { get; set; }
        public int CurrentPage { get; set; } = 1;
        public double TotalPages { get; set; }
        public int PageCount { get; set; }
    }
}
