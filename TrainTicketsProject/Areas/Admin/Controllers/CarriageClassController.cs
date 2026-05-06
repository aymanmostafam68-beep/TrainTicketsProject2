using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class CarriageClassController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private ICarriageClassService _CarriageClassService;


        public CarriageClassController(UserManager<ApplicationUser> userManager, ICarriageClassService carriageClassService)
        {
            _userManager = userManager;
            _CarriageClassService = carriageClassService;
        }

        public async Task<IActionResult> Index(string? ClassName, int page = 1)
        {
            var model = await _CarriageClassService.ClassVMIndex(ClassName, page);
            return View(model);

        }



        public async Task<IActionResult> Create()
        {
            var model = await _CarriageClassService.CreatePage();

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
            var (result, message) = await _CarriageClassService.Create(model, UserId);
            if (!result)
            {
                TempData["Notification-error"] = message;
                return View(model);
            }

         
            TempData["Notification-success"] = "carriageClass added successfully";
           
            return RedirectToAction(nameof(Index), controllerName: "CarriageClass");
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();
             var model = await _CarriageClassService.EditPage(id);


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

             var (result, message) = await _CarriageClassService.EditPost(model, UserId);
            if (result)
            {
                TempData["Notification-success"] = message;
            }
            else
            {
                TempData["Notification-error"] = message;
            }

            return RedirectToAction(nameof(Index), controllerName: "CarriageClass");
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();

            var result = await _CarriageClassService.Delete(id);

            if (result.result)
            {
                TempData["Notification-success"] = result.message;
            }
            else
            {
                TempData["Notification-error"] = result.message;
            }

            return RedirectToAction(nameof(Index), controllerName: "CarriageClass");
        }
    }
}
