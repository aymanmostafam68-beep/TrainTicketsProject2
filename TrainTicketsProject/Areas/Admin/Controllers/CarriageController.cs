using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrainTicketsProject.Models;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class CarriageController : Controller
    {
        private IRepository<Carriage> _repository;
        private IRepository<Train> _trainRepository;
        private IRepository<TrainClass> _trainClassRepository;
        private IRepository<CarriageSeat> _CarriageSeatsRepository;
        private UserManager<ApplicationUser> _userManager;
        private readonly IGetUserInfoEntity _UserInfoService;



        public CarriageController(IRepository<Carriage> repository, IRepository<Train> trainRepository, IRepository<TrainClass> trainClassRepository, IRepository<CarriageSeat> carriageSeatsRepository, UserManager<ApplicationUser> userManager, IGetUserInfoEntity userInfoService)
        {
            _repository = repository;
            _trainRepository = trainRepository;
            _trainClassRepository = trainClassRepository;
            _CarriageSeatsRepository = carriageSeatsRepository;
            _userManager = userManager;
            _UserInfoService = userInfoService;
        }


        public async Task<IActionResult> Index(string? CarriageName, int page = 1)
        {
            var carriages = await _repository.GetAll(null,(items => items.Include(i => i.Train).Include(i => i.Train.Route)),tracked: false);
       

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



            return View(new CarriageVM
            {
                Carriages = carriages.AsEnumerable(),
                CurrentPage = page,
                TotalPages = totalPages
            }

            );
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CarriageCreateVM
            {
                Carriage= new Carriage(),
                Trains = await _trainRepository.GetAll(null, tracked: false),
                TrainClasses = await _trainClassRepository.GetAll(null, tracked: false)
            };

            return View(model);
        }
        [HttpPost]

        public async Task<IActionResult> Create(CarriageCreateVM model)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();
            ModelState.Remove("TrainClasses");

            if (!ModelState.IsValid)
            {

                return View(model); 

            }

            model.Carriage.CreatedUserId = UserId;
            model.Carriage.UpdatedUserId = UserId;

            await _repository.Create(model.Carriage);
            int result = await _repository.Comment();
            if (result > 0)
            {
                TempData["Notification-success"] = "carriage added successfully";
            }
            else
            {
                TempData["Notification-error"] = "Error adding carriage";
            }



            return RedirectToAction(nameof(Index), nameof(Carriage));
        }



        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();

            var carriageData = await _repository.GetOne(c => c.CarriageId == id, tracked: false);
            if (carriageData == null) return NotFound();

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
                
                TrainList = (await _trainRepository.GetAll(null, tracked:false))
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

            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]

        public async Task<IActionResult> Edit(CarriageEditVM model)
        {
            if (model == null) return NotFound();

            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {

                TempData["Notification-error"] = "Error updating carriage";


                return View(model);


            }



            var existingCarriage = await _repository.GetOne(c => c.CarriageId == model.Carriage.CarriageId, tracked: false);
            if (existingCarriage == null) return NotFound();

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
                TempData["Notification-success"] = "Carriage updated successfully";


            }
            else
            {
                TempData["Notification-error"] = "Error updating Carriage";

            }




            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> Delete([FromRoute] int id)
        {
          
            var carriage = await _repository.GetOne(c => c.CarriageId == id, tracked: false, includeFunc: q => q.Include(c => c.CarriageSeats));

            if (carriage is null)
            {
                return NotFound();
            }

            bool hasSeats = carriage.CarriageSeats != null && carriage.CarriageSeats.Any();

            if (carriage.IsActive || hasSeats)
            {
                string reason = carriage.IsActive ? "is currently Active" : "contains registered Seats";
                TempData["Notification-error"] = $"Cannot delete carriage because it {reason}.";

                return RedirectToAction("Index", "Carriage");
            }

            await _repository.Delete(carriage);
            int result = await _repository.Comment();

            if (result > 0)
            {
                TempData["Notification-success"] = "Carriage deleted successfully";
            }
            else
            {
                TempData["Notification-error"] = "Error deleting Carriage";
            }

            return RedirectToAction("Index", "Carriage");
        }



    }
}
