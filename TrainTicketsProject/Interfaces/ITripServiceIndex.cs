namespace TrainTicketsProject.Interfaces
{
    public interface ITripServiceIndex
    {
        Task<TripsInfoList> GetAllTripsWithFullDetailsAsync(string? station, int page);
            }
}
