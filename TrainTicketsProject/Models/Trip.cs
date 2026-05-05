namespace TrainTicketsProject.Models
{
    public class Trip
    {
        [Key]
        public int TripDirectionId { get; set; }

       public DateTime TravelDate { get; set; } = DateTime.Now;
        public int TripScheduleId { get; set; }
        public TripSchedule TripSchedule { get; set; } = default!;

        public bool status { get; set; } = true;

     
    }
}
