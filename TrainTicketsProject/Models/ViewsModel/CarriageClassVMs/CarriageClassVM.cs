namespace TrainTicketsProject.Models.ViewsModel.CarriageClassVMs
{
    public class CarriageClassVM
    {
        public IEnumerable<TrainClass> CarriagesClasses { get; set; } = default!;

        public int CurrentPage { get; set; } = 1;
        public double TotalPages { get; set; }
        public int PageCount { get; set; }
    }
}
