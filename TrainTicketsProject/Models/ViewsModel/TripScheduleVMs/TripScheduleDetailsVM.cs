namespace TrainTicketsProject.ViewModels
{
    public class TripScheduleDetailsVM
    {
        public TripSchedule TripSchedule { get; set; } = new();
        public Route Route { get; set; } = new();
        public Train Train { get; set; } = new();

        public IEnumerable<RouteStation> routeStations = default!;

    }
}