using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainTicketsProject.Models;
using TrainTicketsProject.Models.ViewsModel.CarriageVMs;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class CarraiageSeatController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private ICarraiageSeatService _CarriageSeatsService;

        public CarraiageSeatController(UserManager<ApplicationUser> userManager, ICarraiageSeatService carriageService)
        {
            _userManager = userManager;
            _CarriageSeatsService = carriageService;
        }

        public async Task<IActionResult> Index(string? TrainCode, int page = 1)
        {
           var model = await _CarriageSeatsService.SeatsVMIndex(TrainCode, page);
            return View(model);

        }

        public async Task<IActionResult> Edit([FromRoute] int id)
        {

           var model = await _CarriageSeatsService.EditPage(id);
            return View(model);

        }



        [HttpPost]
        public async Task<IActionResult>  SeatsGenerator(TrainInfoVM model)
        {
          var result = await _CarriageSeatsService.SeatsGenerator(model);
                if (result.result)
                {
                    TempData["Notification-success"] = result.message;
                }
                else
                {
                    TempData["Notification-error"] = result.message;
            }


            return RedirectToAction(nameof(Edit), new { id = model.TrainId });
        }


        [HttpPost]

        public async Task<IActionResult> GenerateSeats([FromRoute]int id) 
        {
            var result = await _CarriageSeatsService.GenerateSeats(id);
            if (result.result)
            {
                TempData["Notification-success"] = result.message;
            }
            else
            {
                TempData["Notification-error"] = result.message;
            }


            return RedirectToAction("Edit", new { id = result.trainId });
        }


        [HttpGet]
        public async Task<IActionResult> ViewSeats([FromRoute] int id)
        {
           
            var model = await _CarriageSeatsService.ViewSeats(id);

            return View(model);


        }

        [HttpPost]
        public async Task<IActionResult> IsOutOfService([FromRoute] int id, int carriageId)
        {
          var result = await _CarriageSeatsService.IsOutOfService(id, carriageId);
            if (result.result)
            {
                TempData["Notification-success"] = result.message;
            }
            else
            {
                TempData["Notification-error"] = result.message;
            }
            return RedirectToAction(nameof(ViewSeats), new { id = carriageId });
        }



    }
}
