
namespace TrainTicketsProject.Models
{
    public enum TicketType
    {
        OneWay = 1,
        RoundTrip = 2
    }


    public class Booking
    {

        [Key]
        public int BookingId { get; set; }
        public string ApplicationUserId { get; set; }


        public int TripScheduleId { get; set; }
        [ForeignKey(nameof(TripScheduleId))]
        public TripSchedule TripSchedule { get; set; } = default!;
        public TicketType TicketType { get; set; }
        public string TicketNumber { get; set; }


        public int? ReturnTripScheduleId { get; set; }
        [ForeignKey(nameof(ReturnTripScheduleId))]
        public TripSchedule? ReturnTripSchedule { get; set; }

        public int CarriageId { get; set; }
        [ForeignKey(nameof(CarriageId))]
        public Carriage Carriage { get; set; } = default!;

        public int CarriageSeatId { get; set; }
        [ForeignKey(nameof(CarriageSeatId))]
        public CarriageSeat CarriageSeat { get; set; } = default!;


        public int? ReturnCarriageId { get; set; }
        [ForeignKey(nameof(ReturnCarriageId))]
        public Carriage? ReturnCarriage { get; set; }

        public int? ReturnCarriageSeatId { get; set; }
        [ForeignKey(nameof(ReturnCarriageSeatId))]
        public CarriageSeat? ReturnCarriageSeat { get; set; }

        public double CalculatedDistance { get; set; } = 0.0;

        public decimal FinalTicketPrice { get; set; }


    }
}
