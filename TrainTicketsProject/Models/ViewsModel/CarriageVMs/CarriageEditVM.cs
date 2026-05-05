using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrainTicketsProject.Models.ViewsModel.CarriageVMs
{
    public class CarriageEditVM
    {
        public Carriage Carriage { get; set; }

        public List<SelectListItem> ClassList { get; set; } = new();

        public List<SelectListItem> TrainList { get; set; } = new();

        [ValidateNever]
        public string createdUserName { get; set; } = string.Empty!;
        [ValidateNever]
        public string CreatedAtInfo { get; set; } = string.Empty!;

        [ValidateNever]
        public string UpdatedAtInfo { get; set; } = string.Empty!;

        [ValidateNever]
        public string updatedUserName { get; set; } = string.Empty!;
    }
}
