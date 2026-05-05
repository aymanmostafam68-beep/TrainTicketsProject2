using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainTicketsProject.ViewModels;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]
    public class TripSchedulesController : Controller
    {
    
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITripScheduleService  _service;

        public TripSchedulesController(SignInManager<ApplicationUser> signInManager, ITripScheduleService service)
        {
            _signInManager = signInManager;
             _service = service;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var model =  await _service.GetIndexVmAsync(page);
            return View(model);
         
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =  await _service.GetCreateVmAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TripScheduleCreateVM model)
        {
            var userId =
                _signInManager.UserManager.GetUserId(User);

            if (userId == null)
                return Unauthorized();

            var result = await  _service.CreateAsync(
                model,
                userId,
                ModelState);

            if (!result.Success)
            {
                TempData["Notification-error"] = result.ErrorMessage;

                return View(model);
            }

            TempData["Notification-success"] =
                "Trip schedule created successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
          var model = await _service.GetEditVmAsync(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TripScheduleCreateVM model)
        {
            var userId =
          _signInManager.UserManager.GetUserId(User);

            if (userId == null)
                return Unauthorized();

            var Result=  await _service.EditAsync( model,userId,ModelState );
            if(!Result.Success)
            {
                TempData["Notification-success"] = Result.Error;

                return View(model) ;
            }

            TempData["Notification-success"] = "Trip schedule updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var Result =  await _service.DeleteAsync(id);
            if(!Result)
            {

                TempData["Notification-Error"] = "Error when try to delete the trip schedule.";

            }

            TempData["Notification-success"] = "Trip schedule deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

       
    }
}
