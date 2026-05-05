using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]
    [AllowAnonymous]
    public class AccountController : Controller
    {


        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private IAccountService _accountService;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAccountService accountService, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            var model = new IdentityVM()
            {
                ReturnURL = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(IdentityVM login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var result = await _userManager.FindByEmailAsync(login.UsernameOrEmail) ??
                   await _userManager.FindByNameAsync(login.UsernameOrEmail);

            if (result == null)
            {
                ModelState.AddModelError("", "Invalid username or email");
                return View(login);
            }
            var signIn = await _signInManager.CheckPasswordSignInAsync(result, login.Password, true);


            if (signIn.IsLockedOut)
            {
                ModelState.AddModelError("", "Your account is locked until " + result.LockoutEnd);
                return View(login);
            }

            if (!result.EmailConfirmed)
            {
                ModelState.AddModelError("", "Please Confirm your email");

                return View(login);
            }

            if (signIn.Succeeded)
            {
                result.LastActivityUtc = DateTime.UtcNow;
                await _userManager.UpdateAsync(result);

                await _signInManager.SignInAsync(result, true);

                TempData["Notification-success"] = "Welcome Back " + result.FirstName;

                return RedirectToAction("Index", "Home", new { area = "Admin" });
                
            }




            //return RedirectToAction("Index", "Home"
            //    , routeValues: new { area = "Customer"
            //    //,Username=result.FirstName,
            //    //emaEmaill=result.Email
            //});
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(login);
        }
    }
}
