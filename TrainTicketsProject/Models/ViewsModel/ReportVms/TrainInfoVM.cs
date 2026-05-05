namespace TrainTicketsProject.Models.ViewsModel.ReportVms
{
    public class TrainInfoVM
    {
        // train name, train code, loop on every carriage and show its name, capacity, and available seats

        public int TrainId { get; set; } = 0;
        public string TrainCode { get; set; } = string.Empty;
        public string TrainName { get; set; }= string.Empty;



        public List<CarriageInfoVM> Carriages { get; set; } = new List<CarriageInfoVM>();


    }
}
