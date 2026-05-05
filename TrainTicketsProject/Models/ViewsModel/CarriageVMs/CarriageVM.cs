namespace TrainTicketsProject.Models.ViewsModel.CarriageVMs
{
    public class CarriageVM
    {
        public IEnumerable<Carriage> Carriages { get; set; } = default!;

        public int CurrentPage { get; set; } = 1;
        public double TotalPages { get; set; }
        public int PageCount { get; set; }
    }
}
