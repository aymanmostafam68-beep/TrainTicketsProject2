namespace TrainTicketsProject.Models
{

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

 
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = default!;
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
        public int PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public List<TransactionEntry> TransactionEntries { get; set; }
    }
}
