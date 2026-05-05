using TrainTicketsProject.Models;

namespace TrainTicketsProject.Interfaces
{
    public interface IStationService
    {
        Task<List<Station>> GetAllStationsAsync();
        Task<Station?> GetStationByCodeAsync(string code);
        Task<bool> CreateStationAsync(Station station);

        Task<Station> UpdateStationAsync(Station station);
        Task<bool> DeleteStationAsync(int id);
    }
}
