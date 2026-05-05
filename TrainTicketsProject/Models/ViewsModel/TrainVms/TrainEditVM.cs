using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TrainTicketsProject.Models.ViewsModel.TrainVms
{
    public class TrainEditVM
    {
        public Train train { get; set; } = default!;

        [ValidateNever]
        public IEnumerable<Route>? Routes { get; set; } = default!;

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
