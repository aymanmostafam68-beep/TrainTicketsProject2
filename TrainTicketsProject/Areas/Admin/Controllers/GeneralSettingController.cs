using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainTicketsProject.ApplicationDB;
using TrainTicketsProject.Models;
using TrainTicketsProject.Models.ViewsModel.CarriageVMs;

namespace TrainTicketsProject.Controllers
{
    [Area(AreaRoles.AdminArea)]
    public class GeneralSettingController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IRepository<GeneralSetting> _SettingRepository;

        private IRepository<Station> _StationRepository;
        private ITimeInterval _IntervalServices;
        private readonly IGetUserInfoEntity _UserInfoService;



        public GeneralSettingController(UserManager<ApplicationUser> userManager, IRepository<GeneralSetting> repository, IRepository<Station> stationRepository, ITimeInterval intervalServices, IGetUserInfoEntity userInfoService)
        {
            _userManager = userManager;
            _SettingRepository = repository;
            _StationRepository = stationRepository;
            _IntervalServices = intervalServices;
            _UserInfoService = userInfoService;
        }



        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var settingsList = await _SettingRepository.GetAll(expression: null,tracked: false);

            int totalItems = settingsList.Count();
            var totalPages = (int)Math.Ceiling(totalItems / 12.0);
            page = page < 1 ? 1 : page;
            page = page > totalPages ? 1 : page;


            settingsList = settingsList
                .Skip((page - 1) * 12)
                .Take(12).ToList();

            var model = new GeneralSettingVM()
            {
                generalSetting = settingsList,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }
        public async Task<IActionResult> Edit([FromRoute] int id) 
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var setting = await _SettingRepository.GetOne(s => s.Id == id);
            if (setting == null) return NotFound();

         var (creator, updater) = await _UserInfoService.GetUserInfo(setting.CreatedUserId, setting.UpdatedUserId);



            var model = new GeneralSettingEditVM()
            {
                generalSetting = setting,
                createdUserName = creator,
                CreatedAtInfo = setting.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAtInfo = setting.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                updatedUserName = updater



            };
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(GeneralSettingEditVM model, IFormFile? logoFile)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var existingSetting = await _SettingRepository.GetOne(x => x.Id == model.generalSetting.Id, tracked: false);

            if (logoFile != null && logoFile.Length > 0)
            {
                _SettingRepository.OnefileUpload(logoFile, "StationLogo", out string newFileName);
                model.generalSetting.logoName = newFileName;
            }
            else
            {
                if (existingSetting != null)
                {
                    model.generalSetting.logoName = existingSetting.logoName;
                }
            }


            model.generalSetting.UpdatedUserId = userId;
            model.generalSetting.UpdatedAt = DateTime.Now;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //check current interval and delete them before create new one
            if(model.generalSetting.TimeSlot != existingSetting.TimeSlot)
            {
                var checkInterval = await _IntervalServices.CreateTimeInterval(model.generalSetting.TimeSlot);
                if (!checkInterval.result)
                {
                    TempData["Notification-error"] = "Failed to create time intervals.";
                    return View(model);
                }
            }

            await _SettingRepository.update(model.generalSetting);
            var result = await _SettingRepository.Comment();

            if (result > 0)
                TempData["Notification-success"] = "Settings updated successfully";
            else
                TempData["Notification-error"] = "No changes were saved.";

            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var hasActiveSetting = await _SettingRepository.GetOne(x => x.IsActive, tracked: false);
            if (hasActiveSetting != null)
            {
                TempData["Notification-error"] = "You cannot create a new setting while an active setting already exists.";
                return RedirectToAction(nameof(Index));
            }


            var model = new GeneralSettingCreateVM()
            {
                generalSetting = new GeneralSetting(),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(GeneralSettingCreateVM model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var hasActiveSetting = await _SettingRepository.GetOne(x => x.IsActive, tracked: false);
            if (hasActiveSetting != null)
            {
                TempData["Notification-error"] = "You cannot create a new setting while an active setting already exists.";
                return RedirectToAction(nameof(Index));
            }

            model.generalSetting.CreatedUserId = userId;
            model.generalSetting.UpdatedUserId = userId;
            model.generalSetting.CreatedAt = DateTime.Now;
            model.generalSetting.UpdatedAt = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _SettingRepository.OnefileUpload(model.LogoFile!, "StationLogo", out string newFileName);
            model.generalSetting.logoName = newFileName;

            var checkInterval = await _IntervalServices.CreateTimeInterval(model.generalSetting.TimeSlot);
            if (!checkInterval.result)
            {
                TempData["Notification-error"] = "Failed to create time intervals.";
                return View(model);
            }

            await _SettingRepository.Create(model.generalSetting);
            var result = await _SettingRepository.Comment();


        




            if (result > 0)
                TempData["Notification-success"] = "Settings created successfully";
            else
                TempData["Notification-error"] = "No changes were saved.";

            return RedirectToAction(nameof(Index));

        }


    }




    }
