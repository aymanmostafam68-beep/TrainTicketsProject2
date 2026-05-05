using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class TripController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITripServiceIndex _Tripservice;

        public TripController(SignInManager<ApplicationUser> signInManager, ITripServiceIndex tripservice)
        {
            _signInManager = signInManager;
            _Tripservice = tripservice;
        }

        public async Task<IActionResult> Index(string? station, int page = 1)
        {
            var model = await _Tripservice.GetAllTripsWithFullDetailsAsync(station, page);

            return View(model);
        }
    }
}
