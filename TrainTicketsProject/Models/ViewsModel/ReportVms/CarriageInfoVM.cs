namespace TrainTicketsProject.Models.ViewsModel.ReportVms
{
    public class CarriageInfoVM
    {

        public int CarriageId { get; set; } = 0;

        public string CarriageName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int AvailableSeats { get; set; }
        public int CreatedSeats { get; set; }

        public DateTime CreatedAt { get; set; } 

        public string TrainClass { get; set; } = string.Empty;


    }
}
