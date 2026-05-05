

using TrainTicketsProject.ViewModels;


namespace TrainTicketsProject.Services
{
    public interface IRouteService
    {

        //get all routes
        Task<List<Route>> GetAllRouteAsync();

        //get route by code
        Task <Route?> GetRouteByCodeAsync(string Code);

        //create new route
        Task<bool> CreateRouteAsync(Route route);

        Task<bool> AddStationToRouteByCodeAsync(string routeCode, string stationCode, decimal distance);

        Task<Route?> GetRouteDetailsAsync(string routeCode);

        Task<bool> CreateRouteWithStationsAsync(Route route, List<StationSelectionRowVM> selectedStations);


        Task<Route> UpdateRouteAsync(Route route, List<StationSelectionRowVM> selectedStations);

        Task<bool> DeleteRouteAsync(int id);


    }
}
