using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TrainTicketsProject.Models.ViewsModel.CarriageVMs
{
    public class CarriageCreateVM 
    {
        public Carriage Carriage { get; set; }

        [ValidateNever]
        public IEnumerable<Train> Trains { get; set; }

        [ValidateNever]
        public IEnumerable<TrainClass> TrainClasses { get; set; }


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
