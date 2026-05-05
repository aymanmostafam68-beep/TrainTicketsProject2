namespace TrainTicketsProject.Interfaces
{
    public interface IGetUserInfoEntity
    {


        Task<(string? creator, string? updater)> GetUserInfo(string? createdUserId, string? updatedUserId);
    }


}
