namespace TrainTicketsProject.Models.ViewsModel.TripScheduleVMs
{
    public class TripSchedulesVM
    {
        public IEnumerable<TripSchedule> TripSchedule { get; set; } = default!;
        public IEnumerable<Route> Route { get; set; } = default!;
        public IEnumerable<Train> Train { get; set; } = default!;
        public int CurrentPage { get; set; }
        public double TotalPages { get; set; }
    }
}