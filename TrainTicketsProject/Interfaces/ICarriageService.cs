using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Interfaces
{
    public interface ICarriageService
    {
        Task<CarriageVM> carriageVMIndex(string? CarriageName, int page = 1);
        Task<CarriageCreateVM> CreatePage();
        bool ValidateCarriage(CarriageCreateVM model);
        Task<(bool result, string message)> Create(CarriageCreateVM model);

        Task<CarriageEditVM> ReloadEditPage(int? id);

        Task<CarriageEditVM> EditPage([FromRoute] int id);

        Task<bool> EditPost(CarriageEditVM model, string UserId);

        Task<(bool result, string message)> Delete([FromRoute] int id);

    }
}
