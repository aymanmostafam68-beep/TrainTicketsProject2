
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Interfaces
{
    public interface ICarriageClassService
    {
        Task<CarriageClassVM> ClassVMIndex(string? ClassName, int page = 1);

        Task<CarriageClassCreateVM> CreatePage();

        Task<(bool result, string? message)> Create(CarriageClassCreateVM model, string UserId);

        Task<CarriageClassEditVM> EditPage([FromRoute] int id);

        Task<(bool result, string? message)> EditPost(CarriageClassEditVM model, string UserId);

        Task<(bool result, string? message)> Delete([FromRoute] int id);
    }
}
