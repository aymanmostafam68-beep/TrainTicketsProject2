using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using static TrainTicketsProject.Services.AccountService;

namespace TrainTicketsProject.Interfaces
{
    public interface IAccountService
    {
        Task<bool> SendConfirmMail(EmailType type, ApplicationUser AppUser, IUrlHelper urlHelper,
            HttpRequest httpRequest);
        Task<bool> ValidateOTPCode(ApplicationUser user, int OTP);
        Task<IdentityResult> ResetPassword(ApplicationUser user, string newPassword);


        AuthenticationProperties ConfigureExternalLogin(string provider, string? redirectUrl);
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<Microsoft.AspNetCore.Identity.SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent);
        Task<IdentityResult> CreateExternalUserAsync(ExternalLoginInfo info);
    }
}
