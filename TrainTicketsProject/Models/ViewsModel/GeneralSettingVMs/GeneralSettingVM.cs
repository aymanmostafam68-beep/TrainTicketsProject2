namespace TrainTicketsProject.Models.ViewsModel.GeneralSettingVMs
{
    public class GeneralSettingVM
    {
        public IEnumerable<GeneralSetting>? generalSetting { get; set; }= default;

        public int CurrentPage { get; set; } = 1;
        public double TotalPages { get; set; }
        public int PageCount { get; set; }
    }
}
