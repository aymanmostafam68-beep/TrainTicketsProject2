using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TrainTicketsProject.Models.ViewsModel.StationVms
{
    public class StationEditVM
    {
        public Station station { get; set; } =default!;

        [ValidateNever]
        public string createdUserName { get; set; } = string.Empty!;
        [ValidateNever]
        public DateTime CreatedAt { get; set; }

        [ValidateNever]
        public DateTime UpdatedAt { get; set; } 


        [ValidateNever]
        public string updatedUserName { get; set; } = string.Empty!;


    }
}
