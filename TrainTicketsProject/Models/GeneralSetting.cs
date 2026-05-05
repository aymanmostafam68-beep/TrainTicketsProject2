namespace TrainTicketsProject.Models
{
    public class GeneralSetting : BaseClass
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? logoName { get; set; } = string.Empty;
     
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price per kilometer must be a positive value.")]
        public decimal PricePerKilometer { get; set; } = 1;
        [Required]
        public int TimeSlot { get; set; } = 30;

    }
}
