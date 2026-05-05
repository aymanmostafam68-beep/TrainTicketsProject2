namespace TrainTicketsProject.Models.ViewsModel.CarriageSeatVMs
{
    public class CarriageSeatEditVM
    {
        public int TrainId { get; set; }
        public string TrainCode { get; set; }
        public string TrainName { get; set; }
        public int CarriageCount { get; set; }
        [Required]
        [Range(1, 80, ErrorMessage = "Seats must be between 1 and 80")]
        public int SeatsPerCarriage { get; set; } = 0;
        public int AvailableSeats { get; set; }
        public int OutOfServiceSeats { get; set; }

        public int TotalSeats { get; set; } = 0;


    }
}
