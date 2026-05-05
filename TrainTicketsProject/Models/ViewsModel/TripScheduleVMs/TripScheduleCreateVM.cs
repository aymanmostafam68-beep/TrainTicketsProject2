using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrainTicketsProject.Models.ViewsModel.TripScheduleVMs
{
    public class TripScheduleCreateVM
    {
        public TripSchedule TripSchedule { get; set; } = new();

        public string? SelectedDepartureTime { get; set; }

        public string? SelectedArrivalTime { get; set; }

        public string AssignedRouteName { get; set; } = string.Empty;

        public bool HasReturnTrip { get; set; }

        public int? ReturnTripScheduleId { get; set; }

        public string? ReturnSelectedDepartureTime { get; set; }

        public string? ReturnSelectedArrivalTime { get; set; }

        public bool ReturnIsNextDay { get; set; }

        public DateTime? ReturnStartDay { get; set; }

        public DateTime? ReturnEndDate { get; set; }

        public int? ReturnEvery { get; set; }
    
        [ValidateNever]

        public IEnumerable<Route> Route { get; set; } = default!;
        [ValidateNever]

        public IEnumerable<Train> Train { get; set; } = default!;

        public IEnumerable<SelectListItem> TimeSlots { get; set; } = Enumerable.Empty<SelectListItem>();


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
