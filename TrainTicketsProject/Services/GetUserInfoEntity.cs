namespace TrainTicketsProject.Services
{
    public class GetUserInfoEntity : IGetUserInfoEntity
    {
       
        private readonly UserManager<ApplicationUser> _userManager;

  
        public GetUserInfoEntity(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
     
        }



        public async Task<(string? creator, string? updater)> GetUserInfo(string? createdUserId, string? updatedUserId)
        {
                string? creator = string.Empty;
                string? updater = string.Empty; 

                if (!string.IsNullOrWhiteSpace(createdUserId))
                {
                   var creatorUser = await _userManager.FindByIdAsync(createdUserId);
                   creator = creatorUser?.UserName;
                }

                if (!string.IsNullOrWhiteSpace(updatedUserId))
                {
                    var updaterUser = await _userManager.FindByIdAsync(updatedUserId);
                    updater = updaterUser?.UserName;
                }

                return (creator, updater);
            }
        
    }
}
