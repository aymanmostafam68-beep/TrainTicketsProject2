using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrainTicketsProject.Models;
using TrainTicketsProject.ViewModels;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class CarriageController : Controller
    {
     
        private UserManager<ApplicationUser> _userManager;
        private ICarriageService _CarriageService;

        public CarriageController(UserManager<ApplicationUser> userManager, ICarriageService carriageService)
        {
            _userManager = userManager;
            _CarriageService = carriageService;
        }

        public async Task<IActionResult> Index(string? CarriageName, int page = 1)
        {
           var model =await _CarriageService.carriageVMIndex(CarriageName, page);

            return View(model);
          
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
           var model =await _CarriageService.CreatePage();
            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> Create(CarriageCreateVM model)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();
            ModelState.Remove("TrainClasses");

            if (!ModelState.IsValid ||! _CarriageService.ValidateCarriage(model))
            {
                await _CarriageService.ReloadEditPage(null);
                return View(model); 

            }

            model.Carriage.CreatedUserId = UserId;
            model.Carriage.UpdatedUserId = UserId;

            var (result, message) = await _CarriageService.Create(model);

      
            if (result)
            {
                TempData["Notification-success"] = message;
            }
            else
            {
                TempData["Notification-error"] = message;
            }



            return RedirectToAction(nameof(Index), nameof(Carriage));
        }

     
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var UserId = _userManager.GetUserId(User);

            if (UserId == null) return Unauthorized();
 
            var model = await _CarriageService.EditPage(id);

            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]

        public async Task<IActionResult> Edit(CarriageEditVM model)
        {
            if (model == null) return NotFound();
            var UserId = _userManager.GetUserId(User);
            if (UserId == null) return Unauthorized();

            if (!ModelState.IsValid || model.Carriage.Capacity <= 0)
            {

                TempData["Notification-error"] = "Error updating carriage";

                 model = await _CarriageService.ReloadEditPage(model.Carriage.CarriageId);
                return View(model);

            }


            var result = await _CarriageService.EditPost(model, UserId);


            if (result)
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
            var UserId = _userManager.GetUserId(User);
            if (UserId == null) return Unauthorized();

          var results = await _CarriageService.Delete(id);

            if (results.result)
            {
                TempData["Notification-success"] = results.message;
            }
            else
            {
                TempData["Notification-error"] = results.message;
            }

            return RedirectToAction("Index", "Carriage");
        }



    }
}
