namespace TrainTicketsProject.Interfaces
{
    public interface IDbInitialize
    {
        Task Initialize();
        Task SeedRolesAsync();
        Task SeedUserAsync(string username, string email, string password, string role);
        Task SeedDataAsync();
    }
}
