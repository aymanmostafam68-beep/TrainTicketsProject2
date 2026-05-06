using Microsoft.AspNetCore.Http.HttpResults;
using TrainTicketsProject.Interfaces;
using TrainTicketsProject.Models;
using TrainTicketsProject.ViewModels;

namespace TrainTicketsProject.Services
{
    public class RouteService : IRouteService
    {

        private readonly IRepository<Route> _routeRepo;
        private readonly IRepository<RouteStation> _routeStationRepo;
        private readonly IRepository<Station> _stationRepo;

        public RouteService (IRepository<Route> routeRepo , IRepository<RouteStation> routeStationRepo , IRepository<Station> stationRep)
        {

            _routeRepo = routeRepo;
            _routeStationRepo = routeStationRepo;
            _stationRepo = stationRep;



        }

        public async Task<List<Route>> GetAllRouteAsync()
        {


          var AllRoutes =   await _routeRepo.GetAll();

            return (AllRoutes);


        }

        public async Task<Route?> GetRouteByCodeAsync(string Code)
        {

            var Route = await _routeRepo.GetOne(
                c => c.Code == Code,
                includeFunc: query => query.Include(

                    r =>r.RouteStations
                    ).ThenInclude(rs =>rs.Station)

                );

            return (Route);

        }


        public async Task<bool> CreateRouteAsync(Route route)
        {
            //check if code is already exist

            var existing =await _routeRepo.GetOne(c => c.Code == route.Code);

            if (existing != null)
            {
                return false;

            }
                



            //if code is new

           await _routeRepo.Create(route);

            var newRoute = await _routeRepo.Comment();


           

                return newRoute > 0;


        }


        public async Task<bool> AddStationToRouteByCodeAsync(string routeCode, string stationCode, decimal distance)
        {

            // use uniqu codes

            var route = _routeRepo.GetOne(r => r.Code == routeCode);
            var station = _stationRepo.GetOne(s => s.Code == stationCode); 


            if (route == null || station == null)
            {
                return false; 

            }


            // create object from RouteStation table 

            var routeStation = new RouteStation
            {
                StationId = station.Id,
                RouteId = route.Id,

                DistanceFromStart = distance  ,

            };

            await _routeStationRepo.Create(routeStation);

            return await _routeStationRepo.Comment() > 0;


        }


        public async Task<Route?> GetRouteDetailsAsync(string routeCode)
        {

            //get route and its stations
            var route = await _routeRepo.GetOne(
                r => r.Code == routeCode,
                includeFunc: query => query.Include(r => r.RouteStations).ThenInclude(rs => rs.Station)
            );

            if (route != null && route.RouteStations != null)
            {
                // Sort the stations by distance before returning the route
                route.RouteStations = route.RouteStations
                    .OrderBy(rs => rs.DistanceFromStart)
                    .ToList();
            }

            return route;
        }



        public async Task<bool> CreateRouteWithStationsAsync(Route route, List<StationSelectionRowVM> selectedStations)
        {
            try
            {
                // 1. Ensure that the code is not repeated
                var existing = await _routeRepo.GetOne(c => c.Code == route.Code);
                if (existing != null) return false;

                // 2. Create the base path
                await _routeRepo.Create(route);
                var saveRouteResult = await _routeRepo.Comment();

                if (saveRouteResult <= 0) return false;

                // 3. Add the selected stations
                if (selectedStations != null && selectedStations.Any())
                {
                    foreach (var selection in selectedStations)
                    {
                        var station = await _stationRepo.GetOne(s => s.Code == selection.StationCode);

                        if (station != null)
                        {
                            var routeStation = new RouteStation
                            {
                                RouteId = route.RouteId,
                                StationId = station.StationId,
                              
                                DistanceFromStart = selection.DistanceFromStart.GetValueOrDefault(0)
                            };

                            await _routeStationRepo.Create(routeStation);
                        }
                    }

                    await _routeStationRepo.Comment();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }




        public async Task<Route> UpdateRouteAsync(Route route, List<StationSelectionRowVM> selectedStations)
        {
            var existingRoute = await _routeRepo.GetOne(
                s => s.RouteId == route.RouteId,
                includeFunc: q => q.Include(r => r.RouteStations)
            );

            if (existingRoute == null) return null;

         
            existingRoute.RouteName = route.RouteName;
            existingRoute.Description = route.Description;
            existingRoute.IsActive = route.IsActive;

            // 1. Delete all old stations from the database for this route
            existingRoute.RouteStations.Clear();

            // 2. Re-add only the Checked stations
            var allStations = await _stationRepo.GetAll();
            foreach (var item in selectedStations)
            {
                var station = allStations.FirstOrDefault(s => s.Code == item.StationCode);
                if (station != null)
                {
                    existingRoute.RouteStations.Add(new RouteStation
                    {
                        RouteId = existingRoute.RouteId,
                        StationId = station.StationId,
                        DistanceFromStart = item.DistanceFromStart ?? 0 
                    });
                }
            }

            await _routeRepo.update(existingRoute);
            await _routeRepo.Comment();

            return existingRoute;
        }




        //delete station

        public async Task<bool> DeleteRouteAsync(int id)
        {

            var route = await _routeRepo.GetOne(c => c.RouteId == id);

            if (route == null || route.IsActive)
            {

                return false;

            }

            await _routeRepo.Delete(route);

            var result = await _routeRepo.Comment();

            return result > 0;

        }

    }
}
