namespace TrainTicketsProject.Models
{
    public class CarriageSeat
    {
        [Key]

        public int CarriageSeatId { get; set; }
        public int SeatNumber { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsOutOfService { get; set; } = false;

        public int CarriageId { get; set; }
        [ForeignKey(nameof(CarriageId))]
        public Carriage Carriage { get; set; } = default!;

    }
}
