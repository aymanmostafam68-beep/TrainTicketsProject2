using Microsoft.AspNetCore.Mvc;
using TrainTicketsProject.Models;
using TrainTicketsProject.Models.ViewsModel.TrainVms;

namespace TrainTicketsProject.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class TrainsController : Controller
    {
        private readonly IRepository<Train> _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Route> _RouteRepo;
        private readonly IGetUserInfoEntity _UserInfoService;






        public TrainsController(IRepository<Train> repo, UserManager<ApplicationUser> userManager, IRepository<Route> routeRepo, IGetUserInfoEntity userInfoService)
        {
            _repo = repo;
            _userManager = userManager;
            _RouteRepo = routeRepo;
            _UserInfoService = userInfoService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var trains = await _repo.GetAll(includeFunc: c => c.Include(t => t.Carriages), tracked: false);

            if (!string.IsNullOrEmpty(search))
                trains = trains.Where(t => t.TrainName.Contains(search)).ToList();

            var model = new TrainVM()
            {
                train = trains,

            };

            return View(model);
        }
        public async Task<IActionResult> Details(int id)
        {
            var train = await _repo.GetOne(
                t => t.TrainId == id,
                q => q.Include(t => t.Route)
            );

            if (train == null)
                return NotFound();

            return View(train);
        }


        public async Task<IActionResult> Create()
        {
            var model = new TrainCreateVM()
            {
                Routes = await _RouteRepo.GetAll(tracked: false)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainCreateVM model)
        {
            var UserId = _userManager.GetUserId(User);
            if (UserId == null)
                return Unauthorized();

            ModelState.Remove("train.Route");
            ModelState.Remove("train.Routes");

            model.train.UpdatedUserId = UserId;
            model.train.CreatedUserId = UserId;
            model.train.UpdatedAt = DateTime.Now;
            model.train.CreatedAt = DateTime.Now;

            if (!ModelState.IsValid)
            {

                var Routes = new TrainCreateVM()
                {
                    Routes = await _RouteRepo.GetAll(tracked: false)
                };
                return View(model);

            }
            var checkCode = await _repo.GetOne(t => t.TrainCode == model.train.TrainCode);
            if (checkCode != null)
            {
                ModelState.AddModelError("train.TrainCode", "The Train Code must be unique");
                model.Routes = await _RouteRepo.GetAll(tracked: false);
                return View(model);
            }




            await _repo.Create(model.train);
          var result =   await _repo.Comment();

            if (result > 0)
            {
                TempData["Notification-success"] = "The Train added successfully";
            }
            else
            {
                TempData["Notification-error"] = "Error while adding The Train";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {

            var train = await _repo.GetOne(t => t.TrainId == id);

            if (train == null)
                return NotFound();
            var (creator, updater) = await _UserInfoService.GetUserInfo(train.CreatedUserId, train.UpdatedUserId);


            var model = new TrainEditVM()
            {
                train = train,
                Routes = await _RouteRepo.GetAll(tracked: false),

                createdUserName = creator ?? "Unknown",
                CreatedAtInfo = train.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                updatedUserName = updater ?? "Unknown",
                UpdatedAtInfo = train.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrainEditVM model)
        {
            var UserId = _userManager.GetUserId(User);
            if (model == null)
                return Unauthorized();

            if (model.train.TrainId == 0)
                return NotFound();

            ModelState.Remove("train.Route");

            if (!ModelState.IsValid)
            {

                model.Routes = await _RouteRepo.GetAll(tracked: false);
                return View(model);

            }
            model.train.UpdatedUserId = UserId;
            model.train.UpdatedAt = DateTime.Now;




            await _repo.update(model.train);
            var result = await _repo.Comment();

            if (result > 0)
            {
                TempData["Notification-success"] = "The Train saved successfully";
            }
            else
            {
                TempData["Notification-error"] = "Error while saving The Train";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var train = await _repo.GetOne(t => t.TrainId == id);

            if (train == null)
                return NotFound();

            return View(train);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var train = await _repo.GetOne(t => t.TrainId == id);

            if (train == null)
                return NotFound();

            await _repo.Delete(train);
            var result = await _repo.Comment();

            if (result > 0)
            {
                TempData["Notification-success"] = "The Train deleted successfully";
            }
            else
            {
                TempData["Notification-error"] = "Error while deleting The Train";
            }


            return RedirectToAction(nameof(Index));
        }
    }
}
