using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Services
{
    public class CarriageClassService : ICarriageClassService
    {
        private IRepository<TrainClass> _repository;
        private readonly IGetUserInfoEntity _UserInfoService;


        public CarriageClassService(IRepository<TrainClass> repository, IGetUserInfoEntity userInfoService)
        {
            _repository = repository;
            _UserInfoService = userInfoService;
        }

        public async Task<CarriageClassVM> ClassVMIndex(string? ClassName, int page = 1)
        {
            var carriageClass = await _repository.GetAll(null, (items => items.Include(i => i.Carriages)), tracked: false);


            if (ClassName is not null)
            {
                carriageClass = carriageClass.Where(e => e.Name.Contains(ClassName)).ToList();
            }
            int totalItems = carriageClass.Count();
            var totalPages = (int)Math.Ceiling(totalItems / 12.0);
            page = page < 1 ? 1 : page;
            page = page > totalPages ? 1 : page;


            carriageClass = carriageClass
                .Skip((page - 1) * 12)
                .Take(12).ToList();




            return new CarriageClassVM
            {
                CarriagesClasses = carriageClass.AsEnumerable(),
                CurrentPage = page,
                TotalPages = totalPages
            };

        }


        public async Task<CarriageClassCreateVM> CreatePage()
        {
            var model = new CarriageClassCreateVM
            {
                CarriageClass = new TrainClass(),

            };

            return model;
        }


        public async Task<(bool result, string? message)> Create(CarriageClassCreateVM model,string UserId)
        {

            model.CarriageClass.CreatedUserId = UserId;

            await _repository.Create(model.CarriageClass);
            int result = await _repository.Comment();
            if (result > 0)
            {
                return (true, "carriageClass added successfully");
            }
            else
            {
                return (false, "Error adding carriageClass");
            }

        }


        public async Task<CarriageClassEditVM> EditPage([FromRoute] int id)
        {
           
            var carriageClass = await _repository.GetOne(c => c.Id == id, tracked: false);
            if (carriageClass == null) return null;

            var (creator, updater) = await _UserInfoService.GetUserInfo(carriageClass.CreatedUserId, carriageClass.UpdatedUserId);

          

            var model = new CarriageClassEditVM
            {
                CarriageClass = carriageClass,
                createdUserName = creator != null ? creator : "Unknown",
                updatedUserName = updater != null ? updater : "Unknown",
                CreatedAtInfo = carriageClass.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAtInfo = carriageClass.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return model;
        }


        public async Task<(bool result, string? message)> EditPost(CarriageClassEditVM model,string UserId)
        {

            var existingClass = await _repository.GetOne(c => c.Id == model.CarriageClass.Id, tracked: false);
            if (existingClass == null) return (false, "Carriage class not found");

            model.CarriageClass.CreatedAt = existingClass.CreatedAt;
            model.CarriageClass.CreatedUserId = existingClass.CreatedUserId;
            model.CarriageClass.UpdatedUserId = UserId;

            await _repository.update(model.CarriageClass);
            int result = await _repository.Comment();
            if (result > 0)
            {
                return (true, "carriageClass updated successfully");
            }
            else
            {
                    return (false, "Error updating carriageClass"); 
            }

        }


        public async Task<(bool result, string? message)> Delete([FromRoute] int id)
        {
            var carriageClass = await _repository.GetOne(
                c => c.Id == id,
                items => items.Include(i => i.Carriages),
                tracked: false);

            if (carriageClass == null) return (false, "Carriage class not found");

            if (carriageClass.Carriages.Any())
            {
                return (false, "You cannot delete this class because it is assigned to one or more carriages.");
            }

            await _repository.Delete(carriageClass);
            int result = await _repository.Comment();

            if (result > 0)
            {
                    return (true, "carriageClass deleted successfully");    
            }
            else
            {
                    return (false, "Error deleting carriageClass"); 
            }

        }

    }
}
