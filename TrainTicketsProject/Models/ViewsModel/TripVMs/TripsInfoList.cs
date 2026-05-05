namespace TrainTicketsProject.Models.ViewsModel.TripVMs
{
    public class TripsInfoList
    {
        public IEnumerable<TripInfo>  tripInfos { get; set; } = default!;

        public int TotalItems { get; set; } = 0;

        public int CurrentPage { get; set; } = 1;
        public double TotalPages { get; set; }
        public int PageCount { get; set; }
    }
}
