namespace TrainTicketsProject.Models
{
    public class RouteStation
    {

        public int Id { get; set;  }

        [ForeignKey("Station")]
        public int StationId { get; set; }

        [ForeignKey("Route")]
        public int RouteId { get; set; }

        public decimal DistanceFromStart { get; set; } 
       
        // Navigation Property

        public Route Route { get; set; }
        public Station Station { get; set; }

    }

}
