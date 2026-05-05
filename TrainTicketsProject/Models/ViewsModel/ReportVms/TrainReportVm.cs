namespace TrainTicketsProject.Models.ViewsModel.ReportVms
{
    public class TrainReportVm
    {
        public IEnumerable<TrainSummaryVM> trains { get; set; } = new List<TrainSummaryVM>();

        public int carriageCount { get; set; } = 0;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
