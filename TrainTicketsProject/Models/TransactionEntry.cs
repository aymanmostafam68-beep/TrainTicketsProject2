namespace TrainTicketsProject.Models
{
    public class TransactionEntry
    {
        public int TransactionEntryId { get; set; }
        public int TransactionId { get; set; }
        public int BookingId { get; set; }
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = default!;
        public decimal Price { get; set; }
        public Transaction Transaction { get; set; } = default!;

   
    }
}
