namespace TrainTicketsProject.Interfaces
{
    public interface ITimeInterval
    {
        Task<(bool result, string? message)> CreateTimeInterval(int timeSlot);
        Task<(bool, int)> GetTimeIntervals(int timeSlot);
        Task<IEnumerable<DepartureTimeInterval>> GetAllTimeIntervals();
    }
}
