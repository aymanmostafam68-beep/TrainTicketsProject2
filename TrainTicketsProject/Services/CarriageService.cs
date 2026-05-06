using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrainTicketsProject.Services
{
    public class CarriageService : ICarriageService
    {
        private IRepository<Carriage> _repository;
        private IRepository<Train> _trainRepository;
        private IRepository<TrainClass> _trainClassRepository;
        private IRepository<CarriageSeat> _CarriageSeatsRepository;
        private readonly IGetUserInfoEntity _UserInfoService;
        public CarriageService(IRepository<Carriage> repository = null, IRepository<Train> trainRepository = null, IRepository<TrainClass> trainClassRepository = null, IRepository<CarriageSeat> carriageSeatsRepository = null, IGetUserInfoEntity userInfoService = null)
        {
            _repository = repository;
            _trainRepository = trainRepository;
            _trainClassRepository = trainClassRepository;
            _CarriageSeatsRepository = carriageSeatsRepository;
            _UserInfoService = userInfoService;
        }


        public async Task<CarriageVM> carriageVMIndex(string? CarriageName, int page = 1)
        {

            var carriages = await _repository.GetAll(null, (items => items.Include(i => i.Train).Include(i => i.Train.Route)), tracked: false);


            if (CarriageName is not null)
            {
                carriages = carriages.Where(e => e.CarriageName.Contains(CarriageName)).ToList();
            }
            int totalItems = carriages.Count();
            var totalPages = (int)Math.Ceiling(totalItems / 12.0);
            page = page < 1 ? 1 : page;
            page = page > totalPages ? 1 : page;


            carriages = carriages
                .Skip((page - 1) * 12)
                .Take(12).ToList();


            return new CarriageVM
            {
                Carriages = carriages.AsEnumerable(),
                CurrentPage = page,
                TotalPages = totalPages
            };

        }


        public async Task<CarriageCreateVM> CreatePage()
        {
            var model = new CarriageCreateVM
            {
                Carriage = new Carriage(),
                Trains = await _trainRepository.GetAll(null, tracked: false),
                TrainClasses = await _trainClassRepository.GetAll(null, tracked: false)
            };

            return model;
        }
        public bool ValidateCarriage(CarriageCreateVM model)
        {
            if (model.Carriage == null || model.Carriage.Capacity <= 0)
            {
                return false;
            }

            return true;
        }

        public async Task<(bool result, string? message)> Create(CarriageCreateVM model)
        {
            await _repository.Create(model.Carriage);
            int result = await _repository.Comment();
            if (result == 0) 
            {
                return (false, "Error adding carriage");
            }
            else
            {
                return (true, "Carriage added successfully");
            }


        }


        public async Task<CarriageEditVM> EditPage([FromRoute] int id)
        {

            var model = await ReloadEditPage(id);

            return model;
        }

        public async Task<bool> EditPost(CarriageEditVM model,string UserId)
        {
            var existingCarriage = await _repository.GetOne(c => c.CarriageId == model.Carriage.CarriageId, tracked: false);
            if (existingCarriage == null) return false;

            model.Carriage.CreatedAt = existingCarriage.CreatedAt;
            model.Carriage.CreatedUserId = existingCarriage.CreatedUserId;

            model.Carriage.UpdatedUserId = UserId;

            await _repository.update(model.Carriage);

            var seats = await _CarriageSeatsRepository.GetAll(
                s => s.CarriageId == existingCarriage.CarriageId,
                tracked: true);

            foreach (var seat in seats)
            {
                seat.IsOutOfService = !model.Carriage.IsActive;
            }

            int result = await _repository.Comment();
            if (result > 0)
            {
                return true;


            }
            else
            {
                return false;


            }


        }



        public async Task<(bool result, string message)> Delete([FromRoute] int id)
        {

            var carriage = await _repository.GetOne(c => c.CarriageId == id, tracked: false, includeFunc: q => q.Include(c => c.CarriageSeats));

            if (carriage is null)
            {
                return (false, "Carriage not found");
            }

            bool hasSeats = carriage.CarriageSeats != null && carriage.CarriageSeats.Any();

            if (carriage.IsActive || hasSeats)
            {
                string reason = carriage.IsActive ? "is currently Active" : "contains registered Seats";
                return (false, $"Cannot delete carriage because it {reason}.");

            }

            await _repository.Delete(carriage);
            int result = await _repository.Comment();

            if (result > 0)
            {
                return (true, "Carriage deleted successfully");
            }
            else
            {
                return (false, "Error deleting Carriage");
            }

        }





        public async Task<CarriageEditVM> ReloadEditPage(int? id)
        {
            var carriageData = await _repository.GetOne(c => c.CarriageId == id, tracked: false);
            var (creator, updater) = await _UserInfoService.GetUserInfo(carriageData.CreatedUserId, carriageData.UpdatedUserId);


            var model = new CarriageEditVM
            {
                Carriage = carriageData,
                ClassList = (await _trainClassRepository.GetAll(null, tracked: false))
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList(),

                TrainList = (await _trainRepository.GetAll(null, tracked: false))
                .Select(t => new SelectListItem
                {
                    Value = t.TrainId.ToString(),
                    Text = t.TrainName
                }).ToList(),
                createdUserName = creator ?? "Unknown",
                CreatedAtInfo = carriageData.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                updatedUserName = updater ?? "Unknown",
                UpdatedAtInfo = carriageData.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")

            };
            return model;


        }




    }
}
