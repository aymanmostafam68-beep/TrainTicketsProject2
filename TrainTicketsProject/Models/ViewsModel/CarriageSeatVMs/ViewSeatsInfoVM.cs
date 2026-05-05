namespace TrainTicketsProject.Models.ViewsModel.CarriageSeatVMs
{
    public class ViewSeatsInfoVM
    {

        public int CarriageId { get; set; } = 0;
        public string CarriageName { get; set; } = string.Empty;
        public int Capacity { get; set; } = 0;

        public List<CarriageSeat>  carriageSeats { get; set; } = new List<CarriageSeat>();
    }
}
