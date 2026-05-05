using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TrainTicketsProject.ViewModels
{
    public class RouteVM : BaseClass
    {
        public int RouteId { get; set; }
        [Required(ErrorMessage = "Route name is required")]
        public string RouteName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; } = string.Empty;
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;

            public List<StationSelectionRowVM> AvailableStations { get; set; } = new();



        [ValidateNever]
        public string createdUserName { get; set; } = string.Empty!;
        [ValidateNever]
        public string CreatedAtInfo { get; set; } = string.Empty!;

        [ValidateNever]
        public string UpdatedAtInfo { get; set; } = string.Empty!;

        [ValidateNever]
        public string updatedUserName { get; set; } = string.Empty!;
    }

    public class StationSelectionRowVM
    {
        public string StationCode { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public decimal? DistanceFromStart { get; set; }
    }
}