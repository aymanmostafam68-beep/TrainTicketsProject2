namespace TrainTicketsProject.Models
{
    public class Route : BaseClass
    {
        //upperEgypt
        //lowerEgypt
        [Key]

        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;

        //  The Fixed Key 
        [Required(ErrorMessage = "Code is required")]

        [RegularExpression(@"^[A-Z]+(_[A-Z0-9]+)*$",
        ErrorMessage = "Use only capital letters and underscore (e.g. Route_2)")]
        public string Code { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string StartPoint { get; set; }

        public string EndPoint { get; set; }


   


        // Navigation Property
        public List<RouteStation> RouteStations { get; set; }
    }
}
