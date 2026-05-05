using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace TrainTicketsProject.Models
{
    public class TripSchedule : BaseClass
    {
        public int TripScheduleId { get; set; }

        public int RouteId { get; set; }
        [ForeignKey(nameof(RouteId))]
        public Route Route { get; set; } = default!;


        public int TrainId { get; set; }
        [ForeignKey(nameof(TrainId))]
        public Train Train { get; set; } = default!;

        public int? ReturnScheduleId { get; set; }
        [ForeignKey(nameof(ReturnScheduleId))]
        public TripSchedule? ReturnSchedule { get; set; }

        public bool IsReturnTrip { get; set; } = false;

        [ValidateNever]
        public string TimeSlot { get; set; } = string.Empty;

        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime { get; set; }

        public bool IsNextDay { get; set; } = false;

        public TimeSpan Duration { get; set; } 

        public DateTime StartDay { get; set; } =DateTime.Now.AddDays(1);
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(2);


        public int Every { get; set; } = 0;



    }
}
