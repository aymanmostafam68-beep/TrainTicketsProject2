namespace TrainTicketsProject.Models.ViewsModel.GeneralSettingVMs
{
    public class GeneralSettingCreateVM
    {
        public GeneralSetting generalSetting { get; set; } = default!;
        [Required(ErrorMessage = "Logo is required.")]
        public IFormFile? LogoFile { get; set; }
        public IEnumerable<Station>? station { get; set; } = default;
    }
}
