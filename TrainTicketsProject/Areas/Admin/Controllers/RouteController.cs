using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using TrainTicketsProject.Models;
using TrainTicketsProject.ViewModels;

namespace TrainTicketsProject.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class RouteController : Controller
    {

        private readonly IRouteService _routeService;
        private readonly IStationService _stationService;

        private readonly UserManager<ApplicationUser> _userManager;




        public RouteController(IRouteService routeService, IStationService stationService, UserManager<ApplicationUser> userManager)
        {

            _routeService = routeService;
            _stationService = stationService;
            _userManager = userManager;
        }

        //==========
        // Help method

        [HttpGet]
        public async Task<IActionResult> GetRouteStations(string code)
        {
            // تأكد أن الـ Service يجلب الـ Stations المرتبطة بالـ Route
            var route = await _routeService.GetRouteByCodeAsync(code);
            if (route == null) return NotFound();

            var stations = route.RouteStations
                .OrderBy(rs => rs.DistanceFromStart)
                .ToList();

            return PartialView("_RouteStationsPartial", stations);
        }


        // Help method so that if the validation fails, the table is no longer blind (without names)
        private async Task RefillAvailableStationsNames(RouteVM vm)
        {
            var stations = await _stationService.GetAllStationsAsync();
            foreach (var item in vm.AvailableStations)
            {
                item.StationName = stations.FirstOrDefault(s => s.Code == item.StationCode)?.Name ?? "";
            }
        }


        //=========================



        public async Task<IActionResult> Index()
        {

            var allRoutes = await _routeService.GetAllRouteAsync();


            var RouteList = allRoutes.Select(r => new RouteVM
            {
                  RouteId = r.RouteId,
                    RouteName = r.RouteName,
                    Description = r.Description,
                    Code = r.Code,
                    StartPoint = r.StartPoint,
                    EndPoint = r.EndPoint,
                    IsActive = r.IsActive
             

            }).ToList();
               
            return View(RouteList);
        }



        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Fetch all stations from the database via service
            var stationsFromDb = await _stationService.GetAllStationsAsync();

            // Initialize the ViewModel with the available stations list
            var viewModel = new RouteVM
            {
                AvailableStations = stationsFromDb.Select(s => new StationSelectionRowVM
                {
                    StationCode = s.Code,
                    StationName = s.Name,
                    IsSelected = false
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RouteVM routevm)
        {

            if (ModelState.IsValid)
            {
                var selectedStations = routevm.AvailableStations
                    .Where(s => s.IsSelected)
                    .ToList();

                var newRoute = new Models.Route
                {
                    RouteName = routevm.RouteName ,
                    Code = routevm.Code,
                    Description = routevm.Description,
                    StartPoint = routevm.StartPoint,
                    EndPoint = routevm.EndPoint,
                };

                var result = await _routeService.CreateRouteWithStationsAsync(newRoute, selectedStations);

                if (result)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Error while saving data.");
            }



            //  rebuild the list on failure to avoid NullReferenceException
            var stationsFromDb = await _stationService.GetAllStationsAsync();
            routevm.AvailableStations = stationsFromDb.Select(s => new StationSelectionRowVM
            {
                StationCode = s.Code,
                StationName = s.Name,

                // preserve the values ​​entered by the user (spaces and selections)

                IsSelected = routevm.AvailableStations?.FirstOrDefault(x => x.StationCode == s.Code)?.IsSelected ?? false,
                DistanceFromStart = routevm.AvailableStations?.FirstOrDefault(x => x.StationCode == s.Code)?.DistanceFromStart ?? 0
            }).ToList();

            return View(routevm);
        }



       


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var UserId= _userManager.GetUserId(User);
            if(UserId == null) return Unauthorized(); 


            var allRoutes = await _routeService.GetAllRouteAsync();  
            var routeBasicInfo = allRoutes.FirstOrDefault(s => s.RouteId == id); 

            if (routeBasicInfo == null) return NotFound();
        
            // 2. get the stations that are actually connected to the path

            var routeWithDetails = await _routeService.GetRouteByCodeAsync(routeBasicInfo.Code);  


            var allStationsFromDb = await _stationService.GetAllStationsAsync(); 

            var routeVM = new RouteVM
            {

                RouteId = routeWithDetails.RouteId,
                RouteName = routeWithDetails.RouteName,
                Description = routeWithDetails.Description,
                StartPoint = routeWithDetails.StartPoint,
                EndPoint = routeWithDetails.EndPoint,
                Code = routeWithDetails.Code,
                IsActive = routeWithDetails.IsActive,

                createdUserName = routeWithDetails.CreatedUserId != null
                    ? (await _userManager.FindByIdAsync(routeWithDetails.CreatedUserId))?.UserName ?? "Unknown"
                    : "Unknown",

                CreatedAtInfo = routeWithDetails.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),

                updatedUserName = routeWithDetails.UpdatedUserId  != null
                    ? (await _userManager.FindByIdAsync(routeWithDetails.UpdatedUserId))?.UserName ?? "Unknown"
                    : "Unknown",

                UpdatedAtInfo = routeWithDetails.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not Updated",


                AvailableStations = allStationsFromDb.Select(s => new StationSelectionRowVM
                {
                    StationCode = s.Code,
                    StationName = s.Name,


                    // We check whether the station code is present in the list of RouteStations returned with Include

                    IsSelected = routeWithDetails.RouteStations.Any(rs => rs.Station.Code == s.Code),



                    // We return the distance if the station is selected, otherwise zero

                    DistanceFromStart = routeWithDetails.RouteStations
                                        .FirstOrDefault(rs => rs.Station.Code == s.Code)?.DistanceFromStart ?? 0
                }).ToList()
            };





            return View(routeVM);
        }


     



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RouteVM routeVM)
        {

            // سينيور نصيحة: شيل الـ Validation بتاع الـ AvailableStations مؤقتاً 
            // عشان نضمن إن الـ ModelState ميعلقش على خانة مسافة لمحطة مش مختارة
            ModelState.Remove("AvailableStations");

            if (!ModelState.IsValid)
            {
                await RefillAvailableStationsNames(routeVM);
                return View(routeVM);
            }
           var UserId = _userManager.GetUserId(User); 
            if(UserId == null) return NotFound(); // Assuming you have a way to get the current user's ID

            var existingRoute = await _routeService.GetRouteByCodeAsync(routeVM.Code);
            if (existingRoute == null) return NotFound();

            // تحديث البيانات الأساسية
            existingRoute.RouteName = routeVM.RouteName;
            existingRoute.Description = routeVM.Description;
            existingRoute.StartPoint = routeVM.StartPoint;
            existingRoute.EndPoint = routeVM.EndPoint;
            existingRoute.IsActive = routeVM.IsActive;
            existingRoute.UpdatedUserId = UserId; // Set the updater
            // تصفية المحطات المختارة فقط بمسافات صالحة
            var selectedStations = routeVM.AvailableStations
                .Where(s => s.IsSelected)
                .ToList();
            routeVM.CreatedAt = existingRoute.CreatedAt; // Preserve original creator
            routeVM.CreatedUserId = UserId; // Set the updater
            var result = await _routeService.UpdateRouteAsync(existingRoute, selectedStations);

            if (result != null)
            {
                TempData["Success"] = "Route updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Internal Error: Could not update Route Stations.");
            await RefillAvailableStationsNames(routeVM);
            return View(routeVM);
        }






        [HttpPost]
        [ValidateAntiForgeryToken] // anti CSRF
        public async Task<IActionResult> Delete(int id)
        {


            try
            {

                var success = await _routeService.DeleteRouteAsync(id);

                if (success)
                {
                    return Json(new { success = true, message = "Route deleted successfully." });
                }

                return Json(new { success = false, message = "Could not delete Route." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Database Error: " + ex.Message });

            }


        }








    }
}
