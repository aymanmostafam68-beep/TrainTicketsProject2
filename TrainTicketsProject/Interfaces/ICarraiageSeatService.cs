using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Interfaces
{
    public interface ICarraiageSeatService
    {
        Task<CarriageSeatVM> SeatsVMIndex(string? TrainCode, int page = 1);
        Task<TrainInfoVM> EditPage([FromRoute] int id);

        Task<(bool result, string? message)> SeatsGenerator(TrainInfoVM model);

        Task<(bool result, string? message, int trainId)> GenerateSeats([FromRoute] int id);

        Task<ViewSeatsInfoVM> ViewSeats([FromRoute] int id);
        Task<(bool result, string? message)> IsOutOfService([FromRoute] int id, int carriageId);
    }
}
