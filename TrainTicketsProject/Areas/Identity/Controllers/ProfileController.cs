using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Areas.Identity.Controllers
{
    [Area(AreaRoles.Identity)]
    [Authorize(Roles = $"{AreaRoles.Admin_Role},{AreaRoles.Super_Admin_Role},{AreaRoles.Customer_Role}")]

    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IAccountService _accountService;


        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAccountService accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await LoadAllVMs(userId); 

            if (model == null) return NotFound();
            return View(model);
        }


      
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileMixedEditVM model)
        {
            string UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.profileEditVM.Id = UserID;

            if (UserID is null) return NotFound();


            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByIdAsync(model.profileEditVM.Id);
            if (user == null) return NotFound();
            user.FirstName = model.profileEditVM.FirstName;
            user.LastName = model.profileEditVM.LastName;
            user.Email = model.profileEditVM.Email;
            user.address = model.profileEditVM.Address;
            user.NationalId = model.profileEditVM.NationalId;
            user.PhoneNumber = model.profileEditVM.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Notification-success"] = "Profile updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);

            }
            return View(model);


        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ProfileMixedEditVM model)
        {
           string UserID =  User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.identityEditVM.ApplicationUserId = UserID;
            model.identityEditVM.Id = UserID;
            if (UserID is null) return NotFound();

            ModelState.Remove("identityEditVM.ApplicationUserId");
            ModelState.Remove("identityEditVM.Id");

            if (!ModelState.IsValid)
            {
                await LoadProfileInfo(model, UserID); 
                return View("Index", model);
            }


            var user = await _userManager.FindByIdAsync(model.identityEditVM.Id);
            if (user == null) return NotFound();
            var identityResult = await _accountService.ResetPassword(user, model.identityEditVM.NewPassword);
            if (identityResult.Succeeded)
            {
                TempData["Notification-success"] = "Password reset successfully! You can now log in with your new password.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ModelState.AddModelError("", errorMessage: string.Join("\n", identityResult.Errors.Select(e => e.Description)));

            }
            await LoadProfileInfo(model, UserID); 
            return View("Index", model);

        }


        private async Task LoadProfileInfo(ProfileMixedEditVM model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                model.profileEditVM = new ProfileEditVM
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Address = user.address,
                    NationalId = user.NationalId,
                    PhoneNumber = user.PhoneNumber
                };
            }
        }

        private async Task<ProfileMixedEditVM?> LoadAllVMs(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new ProfileMixedEditVM
            {
                profileEditVM = new ProfileEditVM
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Address = user.address,
                    NationalId = user.NationalId,
                    PhoneNumber = user.PhoneNumber
                },
                identityEditVM = new IdentityEditVM
                {
                    Id = user.Id,
                    ApplicationUserId = user.Id
                }
            };
        }

    }
}
