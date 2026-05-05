namespace TrainTicketsProject.Models.ViewsModel.TripVMs
{
    public class TripInfo
    {
        public int TripId { get; set; }
        public DateTime TravelDate { get; set; }
        public string TrainCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime { get; set; }

        public TimeSpan Duration { get; set; }


        public List<RouteStation> AllRouteStations { get; set; } = new();


    }
}
