using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static TrainTicketsProject.Services.AccountService;

namespace TrainTicketsProject.Areas.Identity.Controllers
{
    [Area(AreaRoles.Identity)]
    [Authorize(Roles = $"{AreaRoles.Admin_Role},{AreaRoles.Super_Admin_Role},{AreaRoles.Customer_Role}")]

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
        public IActionResult Register(string? returnUrl = null)
        {
            var model = new IdentityCreateVM()
            {
                ReturnURL = returnUrl
            };

            return View(model);

        }

            [HttpPost]
        public async Task<IActionResult> Register(IdentityCreateVM identityVM)
        {
            if (!ModelState.IsValid)
            {
                return View(identityVM);
            }
            var user = new ApplicationUser()
            {
                UserName = identityVM.Email,
                Email = identityVM.Email,
                FirstName = identityVM.FirstName,
                LastName = identityVM.LastName,
                address = identityVM.Address,
                PhoneNumber = identityVM.PhoneNumber
            };
            var identityResult = await _userManager.CreateAsync(user, identityVM.Password);


            if (identityResult.Succeeded)
            {

                var result = await _accountService.SendConfirmMail(EmailType.ConfirmEmail, user, urlHelper: Url, httpRequest: Request);

                TempData["Notification-success"] = "Registration successful! Please check your email to confirm your account.";

                await _userManager.AddToRoleAsync(user, AreaRoles.Customer_Role);


                return RedirectToAction(actionName: "Login");

            }

            else
            {
                ModelState.AddModelError("", string.Join("\n", identityResult.Errors.Select(e=>e.Description)));

            }


            return View();
        }
        [HttpGet]

        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["Notification-success"] = "Email confirmed successfully! You can now log in.";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["Notification-error"] = "Email confirmation failed. Please try again.";
                return RedirectToAction("Login");
            }


        }

        [HttpGet]
        [ActionName("ResendConfirmationMail")]
        public async Task<IActionResult> ResendConfirmationMail()
        {
            return View();

        }

        [HttpPost]

        public async Task<IActionResult> ResendConfirmationMail(ResendConfirmEmail resend)
        {
            var user = await _userManager.FindByEmailAsync(resend.email);


            if (user == null) return NotFound();



            var result = await _accountService.SendConfirmMail(EmailType.ConfirmEmail, user, urlHelper: Url, httpRequest: Request);

            if (result)
            {
                TempData["Notification-success"] = "Please check your email to confirm your account.";

            }

            else
            {
                TempData["Notification-error"] = "email Not found! check your email again";

            }

            return RedirectToAction(actionName: "Login");
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();

        }

        [HttpPost]  

       public async Task<IActionResult> ForgotPassword(ForgotPasswordVM forgotPasswordVM)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordVM.UsernameOrEmail) ??
                 await _userManager.FindByNameAsync(forgotPasswordVM.UsernameOrEmail);
            if (user == null) return NotFound();



            await _accountService.SendConfirmMail(EmailType.ForgotPassword, user, urlHelper: Url, httpRequest: Request);

            TempData["Notification-success"] = $"Welcome {user.FirstName} Please Check Your Email to reset the Password";

            ModelState.AddModelError("", "If an account with that email or username exists, a password reset link has been sent. Please check your email.");    


            return View();

        }

        [HttpGet]

        public async Task<IActionResult> ValidateCode(string Id)
        {

            return View(new OTPCodeVM() { Id = Id });
        }


        public async Task<IActionResult> ValidateCode(OTPCodeVM codeVM)
        {
            var user = await _userManager.FindByIdAsync(codeVM.Id);
            if (user == null) return NotFound();
            var isValid = await _accountService.ValidateOTPCode(user, codeVM.OTP);
            if (isValid)
            {
                TempData["Notification-success"] = "OTP code is valid. You can now reset your password.";
                return RedirectToAction("ResetPassword", new { ApplicationUserId = codeVM.Id });
            }
            else
            {
                ModelState.AddModelError("", "Invalid OTP code. Please try again.");
                return View(codeVM);
            }


        }



        [HttpGet]
        public async Task<IActionResult> ResetPassword(string ApplicationUserId)
        {

            return View(new IdentityEditVM() { ApplicationUserId = ApplicationUserId });
        }



        [HttpPost]
        public async Task<IActionResult> ResetPassword(IdentityEditVM changePasswordVM)
        {
            var user = await _userManager.FindByIdAsync(changePasswordVM.ApplicationUserId);
            if (user == null) return NotFound();
            var identityResult = await _accountService.ResetPassword(user, changePasswordVM.NewPassword);
            if (identityResult.Succeeded)
            {
                TempData["Notification-success"] = "Password reset successfully! You can now log in with your new password.";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("", string.Join("\n", identityResult.Errors.Select(e => e.Description)));
                return View(changePasswordVM);
            }

        }






        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
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

                return RedirectToAction("Index", "Profile"
                    , routeValues: new
                    {
                        area = "Identity"
                        //    //,Username=result.FirstName,
                        //    //emaEmaill=result.Email
                    });
            }
         



            //return RedirectToAction("Index", "Home"
            //    , routeValues: new { area = "Customer"
            //    //,Username=result.FirstName,
            //    //emaEmaill=result.Email
            //});
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(login);
        }
        [HttpGet]
        public IActionResult Logout()
        {
            return View();
        }



        [HttpPost]
        [ActionName("Logout")]
        public async Task<IActionResult> Logout(string url=null)
        {
            await _signInManager.SignOutAsync();
            TempData["Notification-success"] = "You have been logged out.";
            return RedirectToAction("Logout", "Account", new { area = "Identity" });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Generate the callback URL for to redirect after login
            // This URL includes the returnUrl as a route parameter, which will be used to redirect the user
            // back to the original page they were trying to access after a successful external login.
            var redirectUrl = Url.Action(
                action: "ExternalLoginCallback", // The name of the callback action method.
                controller: "Account",
                         // The name of the controller containing the callback method.
                values: new { ReturnUrl = returnUrl } // Pass the returnUrl as a parameter to the callback method.
            );

            // Configure authentication parameters for the external login.
            var properties = _accountService.ConfigureExternalLogin(provider, redirectUrl);

            // Redirect the user to the external provider's login page (e.g., Google or Facebook).
            // The "ChallengeResult" triggers the external authentication process, which redirects the user
            // to the external provider's login page using the configured properties.
            return new ChallengeResult(provider, properties);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            // If no returnUrl is provided, default to the application's home page.
            returnUrl = returnUrl ?? Url.Content("~/");

            // Check if an error occurred during the external authentication process.
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(actionName: "Login");
            }

            // Retrieve login information about the user from the external login provider.
            var info = await _accountService.GetExternalLoginInfoAsync();

            // If the login information could not be retrieved, display an error message
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return RedirectToAction("Login");
            }

            // Attempt to sign in the user using their external login details.
            var result = await _accountService.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

            // If the external login succeeds, redirect the parent window to the returnUrl
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Profile");
            }
        
            // If the user does not have a corresponding record in the UserLogins table create a new account
            var createResult = await _accountService.CreateExternalUserAsync(info);
            if (createResult.Succeeded)
                return RedirectToAction("Index", "Profile");
            foreach (var error in createResult.Errors)
                ModelState.AddModelError("", error.Description);

            return View("Error");
        }








    }
}
