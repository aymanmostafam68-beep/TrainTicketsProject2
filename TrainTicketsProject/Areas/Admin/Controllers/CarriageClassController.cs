using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class CarriageClassController : Controller
    {
        private IRepository<TrainClass> _repository;
        private UserManager<ApplicationUser> _userManager;


        public CarriageClassController(IRepository<TrainClass> repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? ClassName, int page = 1)
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




            return View(new CarriageClassVM
            {
                CarriagesClasses = carriageClass.AsEnumerable(),
                CurrentPage = page,
                TotalPages = totalPages
            }

            );
        }



        public async Task<IActionResult> Create()
        {
            var model = new CarriageClassCreateVM
            {
                CarriageClass = new TrainClass(),

            };

            return View(model);
        }
        [HttpPost]

        public async Task<IActionResult> Create(CarriageClassCreateVM model)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {

                return View(model);

            }

            model.CarriageClass.CreatedUserId = UserId;
           
            await _repository.Create(model.CarriageClass);
            int result = await _repository.Comment();
            if (result > 0)
            {
                TempData["Notification-success"] = "carriageClass added successfully";
            }
            else
            {
                TempData["Notification-error"] = "Error adding carriageClass";
            }



            return RedirectToAction(nameof(Index), controllerName: "CarriageClass");
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();
            var carriageClass = await _repository.GetOne(c => c.Id == id, tracked: false);
            if (carriageClass == null) return NotFound();

            var createdUser = await _userManager.FindByIdAsync(carriageClass.CreatedUserId);
            var updatedUser = await _userManager.FindByIdAsync(carriageClass.UpdatedUserId);



            var model = new CarriageClassEditVM
            {
                CarriageClass = carriageClass,
                createdUserName = createdUser != null ? createdUser.UserName : "Unknown",
                updatedUserName = updatedUser != null ? updatedUser.UserName : "Unknown",
                CreatedAtInfo = carriageClass.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAtInfo = carriageClass.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CarriageClassEditVM model)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();


            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingClass = await _repository.GetOne(c => c.Id == model.CarriageClass.Id, tracked: false);
            if (existingClass == null) return NotFound();

            model.CarriageClass.CreatedAt = existingClass.CreatedAt;
            model.CarriageClass.CreatedUserId = existingClass.CreatedUserId;
            model.CarriageClass.UpdatedUserId = UserId;

            await _repository.update(model.CarriageClass);
            int result = await _repository.Comment();
            if (result > 0)
            {
                TempData["Notification-success"] = "carriageClass updated successfully";
            }
            else
            {
                TempData["Notification-error"] = "Error updating carriageClass";
            }

            return RedirectToAction(nameof(Index), controllerName: "CarriageClass");
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var carriageClass = await _repository.GetOne(
                c => c.Id == id,
                items => items.Include(i => i.Carriages),
                tracked: false);

            if (carriageClass == null) return NotFound();

            if (carriageClass.Carriages.Any())
            {
                TempData["Notification-error"] = "You cannot delete this class because it is assigned to one or more carriages.";
                return RedirectToAction(nameof(Index), controllerName: "CarriageClass");
            }

            await _repository.Delete(carriageClass);
            int result = await _repository.Comment();

            if (result > 0)
            {
                TempData["Notification-success"] = "carriageClass deleted successfully";
            }
            else
            {
                TempData["Notification-error"] = "Error deleting carriageClass";
            }

            return RedirectToAction(nameof(Index), controllerName: "CarriageClass");
        }
    }
}
