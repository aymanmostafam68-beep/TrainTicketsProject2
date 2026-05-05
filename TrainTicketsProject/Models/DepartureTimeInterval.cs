namespace TrainTicketsProject.Models
{
    public class DepartureTimeInterval
    {
        public string Id { get; set; } = string.Empty;
     
        public TimeSpan DepartureTime { get; set; }

        public bool IsSelected { get; set; }= false;
    }

 
}
