using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;


namespace TrainTicketsProject.Services
{
    public class AccountService : IAccountService
    {
        public enum EmailType
        {

            ConfirmEmail,
            ChangePasswordEmail,
            ForgotPassword

        };

        public AccountService(UserManager<ApplicationUser> userManager,
             SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender, IRepository<ApplicationUserOTP> OTP

            )
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _OTP = OTP;
            _signInManager = signInManager;
        }

        public UserManager<ApplicationUser> _userManager { get; }
        public IEmailSender _emailSender { get; }
        public IRepository<ApplicationUserOTP> _OTP { get; }
        SignInManager<ApplicationUser> _signInManager { get; }

        public async Task<bool> SendConfirmMail(EmailType type, ApplicationUser AppUser, IUrlHelper urlHelper,
            HttpRequest httpRequest)
        {

            //1
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(AppUser);

            var confirmationlink = string.Empty;
            var subject = string.Empty;
            var body = string.Empty;


            if (type == EmailType.ConfirmEmail)
            {
                subject = "Confirm your email";
                confirmationlink = urlHelper.Action(
                "ConfirmEmail",
                "Account",
            new { area = "Identity", AppUser.Id, token }, protocol: httpRequest.Scheme);

                body = $"Please confirm your account by <a href='{confirmationlink}'>clicking here</a>.";

            }

            if (type == EmailType.ForgotPassword)
            {

                int OTP = Random.Shared.Next(33333, 99999);


                await _userManager.SetAuthenticationTokenAsync(AppUser, "Default", "PasswordResetToken", token);
                await _OTP.Create(new ApplicationUserOTP()
                {
                    OTP = OTP.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false,
                    IsValid = true,
                    ApplicationUserId = AppUser.Id,
                    ApplicationUserName = AppUser.UserName
                });
                await _OTP.Comment();

                subject = "Reset Password";
                confirmationlink = urlHelper.Action(
                "ValidateCode",
                "Account",
                 new { area = "Identity", AppUser.Id }, protocol: httpRequest.Scheme);
                body = $"Your OTP Code is  {OTP}  To Confirm The OTP and reset the password click here <a href='{confirmationlink}'>Reset Password</a>.";

            }



            try
            {
                //3
                await _emailSender.SendEmailAsync(
                  AppUser.Email!,
                   subject,
                 body);
                return true;
            }
            catch (Exception)
            {
                return false;

            }


        }

        public async Task<bool> ValidateOTPCode(ApplicationUser user, int OTP)
        {
            if (user == null) return false;
            if (OTP == 0) return false;

            ApplicationUserOTP? ApplicationUserOTP = _OTP.GetAll(o => o.ApplicationUserId == user.Id && o.OTP == OTP.ToString() && o.IsUsed == false && o.IsValid == true)
                .Result.OrderBy(e => e.id).LastOrDefault();

            if (ApplicationUserOTP == null) return false;
            if (ApplicationUserOTP.ExpireAt <= DateTime.UtcNow) return false;
            ApplicationUserOTP.IsUsed = true;
            ApplicationUserOTP.IsValid = false;

            await _OTP.update(ApplicationUserOTP);
            await _OTP.Comment();
            return true;

        }


        public async Task<IdentityResult> ResetPassword(ApplicationUser user, string newPassword)
        {
            if (user == null)
                return IdentityResult.Failed(
                                                new IdentityError { Description = "User not found." });
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            return result;
        }

        public AuthenticationProperties ConfigureExternalLogin(string provider, string? redirectUrl)
        {
            // The ConfigureExternalAuthenticationProperties method sets up parameters needed for the external provider,
            // such as the login provider name (e.g., Google, Facebook) and the redirect URL to be used after login.
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            // Retrieve login information about the user from the external login provider (e.g., Google, Facebook).
            // This includes details like the provider's name and the user's identifier within that provider.
            return await _signInManager.GetExternalLoginInfoAsync();
        }

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent)
        {
            // Attempt to sign in the user using their external login details.
            // If a corresponding record exists in the UserLogins table, the user will be logged in.
            return await _signInManager.ExternalLoginSignInAsync(
                loginProvider,    // The name of the external login provider (e.g., Google, Facebook).
                providerKey,      // The unique identifier of the user within the external provider.
                isPersistent: isPersistent,   // Indicates whether the login session should persist across browser restarts.
                bypassTwoFactor: true  // Bypass two-factor authentication if enabled.
            );
        }

        public async Task<IdentityResult> CreateExternalUserAsync(ExternalLoginInfo info)
        {
            // Extract email claim (mandatory for identifying the user)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return IdentityResult.Failed(new IdentityError { Description = "Email not received from external provider." });

            // Check if user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // If user already exists, link this external login to the existing account
                var loginResult = await _userManager.AddLoginAsync(existingUser, info);

                if (loginResult.Succeeded)
                {
                    // Update last login time
                    existingUser.LastActivityUtc = DateTime.UtcNow;
                    await _userManager.UpdateAsync(existingUser);

                    // Sign in the existing user
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                }

                return loginResult;
            }

            // Extract optional claims
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

            // Create new ApplicationUser instance
            var user = new ApplicationUser
            {
                UserName = email,                     // Use email as username (unique in system)
                Email = email,                             // Primary email from Google
                FirstName = firstName,              // From Google claim (or blank if missing)
                LastName = lastName,               // From Google claim (nullable)
                EmailConfirmed = true,              // External providers already confirm email
                //IsActive = true,
                //CreatedOn = DateTime.UtcNow,
                LastActivityUtc = DateTime.UtcNow
            };

            // Create the user in Identity DB (Users table)
            var result = await _userManager.CreateAsync(user);
            await _userManager.AddToRoleAsync(user, AreaRoles.Customer_Role);

            if (!result.Succeeded)
                return result;

            // Link external login info (UserLogins table)
            result = await _userManager.AddLoginAsync(user, info);

            // Sign in immediately if successful
            if (result.Succeeded)
                await _signInManager.SignInAsync(user, isPersistent: false);

            return result;
        }


    }
}
