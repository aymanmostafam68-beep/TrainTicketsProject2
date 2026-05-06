namespace TrainTicketsProject.Services
{
    public class TripServiceIndex : ITripServiceIndex
    {
        public readonly IRepository<Trip> _tripRepository;

        public TripServiceIndex(IRepository<Trip> tripRepository)
        {
            _tripRepository = tripRepository;
        }

        public async Task<TripsInfoList> GetAllTripsWithFullDetailsAsync(string? station,int page)
        {
            var trips = await _tripRepository.GetAll(
                c=>c.TravelDate>DateTime.Now , 
                q => q.Include(t => t.TripSchedule)
                        .ThenInclude(s => s.Train)
                      .Include(t => t.TripSchedule)
                        .ThenInclude(s => s.Route)
                          .ThenInclude(r => r.RouteStations)
                            .ThenInclude(rs => rs.Station),
                tracked: false);
            if (!string.IsNullOrWhiteSpace(station))
            {
                trips = trips.Where(t => t.TripSchedule.Route.RouteStations
                                       .Any(rs => rs.Station.Name.Contains(station, StringComparison.OrdinalIgnoreCase)))
                                       .ToList();
            }




            var allTripInfos = trips.Select(t => {
                var schedule = t.TripSchedule;
                var orderedStations = schedule.Route.RouteStations
                                              .OrderBy(rs => rs.DistanceFromStart)
                                              .ToList();

             return new TripInfo
            {
                TripId = t.TripDirectionId,
                TrainCode = schedule.Train?.TrainCode ?? "N/A",
                RouteName = schedule.Route?.RouteName ?? "N/A",
                 DepartureStation = schedule.IsReturnTrip ? orderedStations.LastOrDefault()?.Station?.Name ?? "N/A" : orderedStations.FirstOrDefault()?.Station?.Name ?? "N/A",
                 ArrivalStation = schedule.IsReturnTrip ? orderedStations.FirstOrDefault()?.Station?.Name ?? "N/A" : orderedStations.LastOrDefault()?.Station?.Name ?? "N/A",
                 DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                Duration = schedule.Duration,
                TravelDate = t.TravelDate,
                 AllRouteStations = schedule.IsReturnTrip ? orderedStations.AsEnumerable().Reverse().ToList() : orderedStations.ToList()
             };
        }).OrderBy(t => t.TravelDate);


            int pageSize = 12;
            var totalCount = allTripInfos.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            page = page < 1 ? 1 : page;
            page = page > totalPages ? 1 : page;

            var paginatedTrips = allTripInfos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            return new TripsInfoList
            {
                tripInfos = paginatedTrips,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalCount
            };





        }
    }
}
