namespace TrainTicketsProject.ApplicationDB
{
    public class DbInitialize : IDbInitialize
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDataAccess _dataAccess;

        public DbInitialize(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDataAccess dataAccess)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;
            this._dataAccess = dataAccess;
        }

        public async Task Initialize()
        {


            await SeedDataAsync();
            await SeedRolesAsync();
            await SeedUserAsync("superadmin", "superadmin@mail.com", "SuperAdmin123*", AreaRoles.Super_Admin_Role);
            await SeedUserAsync("admin", "admin@mail.com", "Admin123*", AreaRoles.Admin_Role);
            await SeedUserAsync("employee", "employee@mail.com", "Employee123*", AreaRoles.Employee_Role);
            await SeedUserAsync("customer", "customer@mail.com", "Customer123*", AreaRoles.Customer_Role);


        }

        public async Task SeedDataAsync()
        {
            if ((await _dataAccess.Database.GetPendingMigrationsAsync()).Any())
            {
                await _dataAccess.Database.MigrateAsync();
            }
        }



        public async Task SeedRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync(AreaRoles.Super_Admin_Role))
                await _roleManager.CreateAsync(new IdentityRole(AreaRoles.Super_Admin_Role));

            if (!await _roleManager.RoleExistsAsync(AreaRoles.Admin_Role))
                await _roleManager.CreateAsync(new IdentityRole(AreaRoles.Admin_Role));

            if (!await _roleManager.RoleExistsAsync(AreaRoles.Employee_Role))
                await _roleManager.CreateAsync(new IdentityRole(AreaRoles.Employee_Role));

            if (!await _roleManager.RoleExistsAsync(AreaRoles.Customer_Role))
                await _roleManager.CreateAsync(new IdentityRole(AreaRoles.Customer_Role));
        }


        public async Task SeedUserAsync(string username, string email, string password, string role)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user is null)
            {
                var newUser = new ApplicationUser
                {
                    FirstName = username,
                    LastName = username,
                    Email = email,
                    EmailConfirmed = true,
                    UserName = username
                };
                var result = await _userManager.CreateAsync(newUser, password);
                if (!result.Succeeded)
                    return;
                user = newUser;

            }


            if (!await _userManager.IsInRoleAsync(user, role))
                await _userManager.AddToRoleAsync(user, role);
        }
    }
}
