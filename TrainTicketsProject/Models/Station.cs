using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TrainTicketsProject.Models
{
    public class Station : BaseClass
    {
        [Key]

        public int StationId { get; set; }
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        
        //  The Fixed Key 
        [Required(ErrorMessage = "Code is required")]

        [RegularExpression(@"^[A-Z]+(_[A-Z0-9]+)*$",
        ErrorMessage = "Use only capital letters and underscore (e.g. Station_2)")]
        public string Code { get; set; }




        // Navigation Property
        [ValidateNever]
        public List<RouteStation>? RouteStations { get; set; }

        [ValidateNever]
        public List<GeneralSetting>? GeneralSettings { get; set; }
    }
}

