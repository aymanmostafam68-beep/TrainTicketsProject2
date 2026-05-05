namespace TrainTicketsProject.ViewModels
{
    public class DashboardVM
    {
        public int TotalTrips { get; set; }
        public int TotalBookings { get; set; }
        public int TotalTrains { get; set; }
        public double OccupancyRate { get; set; }

        public int TotalTransactions { get; set; }
    }
}
