using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace TrainTicketsProject.Services
{
    public class TimeInterval : ITimeInterval
    {
        private IRepository<DepartureTimeInterval> _timeIntervalRepository;
        private IRepository<GeneralSetting> _generalSettingRepository;



        public TimeInterval(IRepository<DepartureTimeInterval> timeIntervalRepository, IRepository<GeneralSetting> generalSettingRepository)
        {
            _timeIntervalRepository = timeIntervalRepository;
            _generalSettingRepository = generalSettingRepository;
        }


        //return time intervals based on the general setting of the system

        public async Task<(bool, int)> GetTimeIntervals(int timeSlot)
        {
            if (timeSlot <= 0)
            {
                return (false, timeSlot);
            }

            return (true, timeSlot);

        }

        public async Task<(bool result, string? message)> CreateTimeInterval(int timeSlot)
        {
            if (timeSlot <= 0)
            {
                return (false, "Time slot must be greater than 0.");
            }

            const int totalMinutesPerDay = 24 * 60;

            var existingIntervals = await _timeIntervalRepository.GetAll();

            if (existingIntervals.Count() != 0)
            {
                await _timeIntervalRepository.DeleteRange(existingIntervals);
            }

            var timeIntervals = Enumerable
                .Range(0, totalMinutesPerDay / timeSlot)
                .Select(i => new DepartureTimeInterval
                {
                    Id = Guid.NewGuid().ToString(),
                    DepartureTime = TimeSpan.FromMinutes(i * timeSlot),
                    IsSelected = false
                })
                .ToList();

            await _timeIntervalRepository.AddRange(timeIntervals);

            await _timeIntervalRepository.Comment();

            return (true, null);
        }

        public async Task<IEnumerable<DepartureTimeInterval>> GetAllTimeIntervals()
        {
            return await _timeIntervalRepository.GetAll(tracked: false);
        }
    }
}
