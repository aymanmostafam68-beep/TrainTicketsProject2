using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TrainTicketsProject.Models.ViewsModel.TrainVms
{
    public class TrainCreateVM
    {
        public Train train { get; set; } = default!;

        [ValidateNever]
        public IEnumerable<Route>? Routes { get; set; } = default!;
    }
}
