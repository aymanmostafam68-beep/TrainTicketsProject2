namespace TrainTicketsProject.Models.ViewsModel
{
    public class TrainSummaryVM
    {
        public int id { get; set; }
        public string TrainCode { get; set; }
        public string TrainName { get; set; }
        public int CarriageCount { get; set; }
        public int Capacity { get; set; }
        public int SeatCount { get; set; }
        public int AvailableSeats { get; set; }

        public string Route { get; set; } = string.Empty;

        public bool IsActive { get; set; }= false;


    }
}
