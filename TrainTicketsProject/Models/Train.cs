using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TrainTicketsProject.Models
{
    public class Train : BaseClass
    {
        [Key]

        public int TrainId { get; set; }
        public string TrainName { get; set; } = string.Empty;
        public string TrainCode { get; set; } = string.Empty;


        public int RouteId { get; set; }
        [ForeignKey(nameof(RouteId))]
        public Route Route { get; set; } = default!;

        [ValidateNever]
        public ICollection<Carriage> Carriages { get; set; } = new List<Carriage>();


    }
}
