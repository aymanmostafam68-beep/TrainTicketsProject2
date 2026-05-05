using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrainTicketsProject.Models;
using TrainTicketsProject.ViewModels;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]
    public class StationController : Controller
    {

        private readonly IStationService _stationService;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGetUserInfoEntity _UserInfoService;

        public StationController(IStationService stationService, UserManager<ApplicationUser> userManager, IGetUserInfoEntity userInfoService)
        {
            _stationService = stationService;
            _userManager = userManager;
            _UserInfoService = userInfoService;
        }

        public async Task<IActionResult> Index()
        {

            var stations = await _stationService.GetAllStationsAsync();



            // send station to viewmodel

            var StationsList = stations.Select(s => new StationVM

            {
                station = new Station
                {
                    StationId = s.StationId,
                    Name = s.Name,
                    Code = s.Code,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    
                }


            }).ToList();



            return View(StationsList);
        }

        [HttpGet]
        public async Task<IActionResult> Create( )
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken] // anti CSRF
        public async Task<IActionResult> Create(StationCreateVM stationVM)
        {
            var UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {

                var station = new Station
                {

                    StationId = stationVM.station.StationId,
                    Name = stationVM.station.Name , 
                    Code = stationVM.station.Code,
                    Description = stationVM.station.Description , 
                    CreatedAt = DateTime.Now ,
                    UpdatedAt = DateTime.Now,
                    IsActive = stationVM.station.IsActive ,
                    CreatedUserId = UserId,
                    UpdatedUserId = UserId

                };


                var result =  await _stationService.CreateStationAsync(station);

                if (result)
                {
                    TempData["Notification-success"] = "Station created successfully";

                } else
                {
                    TempData["Notification-error"] = "Error creating station. Please try again.";
                    ModelState.AddModelError("", "The station code is already exist!");


                }


                return RedirectToAction(nameof(Index));

            }


            return View(stationVM);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //  get all the stations and map the match with the ID
            var allStations = await _stationService.GetAllStationsAsync();
            var station = allStations.FirstOrDefault(s => s.StationId == id);

            if (station == null) return NotFound();

            var (creator, updater) = await _UserInfoService.GetUserInfo(station.CreatedUserId, station.UpdatedUserId);


            var stationVM = new StationEditVM
            {
                station = new Station
                {
                    StationId = station.StationId,
                    Name = station.Name,
                    Description = station.Description,
                    Code = station.Code,
                    IsActive = station.IsActive
                },
                // Use ?. and ?? to handle cases where the user is not found in the DB
                createdUserName = station.CreatedUserId != null
          ? (await _userManager.FindByIdAsync(station.CreatedUserId))?.UserName ?? "Unknown"
          : "Unknown",

                CreatedAt = station.CreatedAt,

                updatedUserName = station.UpdatedUserId != null
          ? (await _userManager.FindByIdAsync(station.UpdatedUserId))?.UserName ?? "Unknown"
          : "Unknown",

                // Use ?. for ToString if UpdatedAt is nullable
                UpdatedAt = station.UpdatedAt
            };

            return View(stationVM);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StationEditVM stationVM)
        {
            var UserId = _userManager.GetUserId(User);
            if(UserId == null) return Unauthorized();

            if (!ModelState.IsValid) return View(stationVM);

       
            var existingStation = await _stationService.GetStationByCodeAsync(stationVM.station.Code);

            if (existingStation == null)
            {
                ModelState.AddModelError("", "Station not found.");
                return View(stationVM);
            }

          
            existingStation.Name = stationVM.station.Name;
            existingStation.Description = stationVM.station.Description;
            existingStation.IsActive = stationVM.station.IsActive;
            existingStation.UpdatedAt = DateTime.Now;
            existingStation.UpdatedUserId = UserId;
            existingStation.CreatedUserId = existingStation.CreatedUserId; // Preserve original creator
            existingStation.CreatedAt = existingStation.CreatedAt; //  Preserve original creation time

            var result = await _stationService.UpdateStationAsync(existingStation);

            if (result != null)
            {
                TempData["Notification-success"] = "Station updated successfully";
                return RedirectToAction(nameof(Index));


            }
            else
            {
                TempData["Notification-error"] = "Error updating station. Please try again.";
                ModelState.AddModelError("", "Error saving changes.");

                return View(stationVM);

            }






        }







        [HttpPost]
        [ValidateAntiForgeryToken] // anti CSRF
        public async Task<IActionResult> Delete(int id)
        {

            try
            {

               var success = await _stationService.DeleteStationAsync(id);



                if (success)
                {
                    return Json(new { success = true, message = "Station deleted successfully." });
                }
                TempData["Notification-error"] = "Could not delete station because it is active or linked to existing routes..";

                return Json(new { success = false, message = "Could not delete station because it is active or linked to existing routes." });

            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Database Error: " + ex.Message });

            }


        }


    }
}
