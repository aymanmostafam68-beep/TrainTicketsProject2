using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;


        private readonly ApplicationDB.ApplicationDataAccess _context;


        public UserController(UserManager<ApplicationUser> userManager,
          ApplicationDB.ApplicationDataAccess context)

        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string? UserName, string? roleName, int page = 1)
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserVM
                {
                    id = user.Id,
                    Email = user.Email,
                    userName = user.UserName,
                    RoleName = roles.FirstOrDefault(),
                    IsConfirmed = user.EmailConfirmed,
                    IsLocked = user.LockoutEnd > DateTime.UtcNow,
                    LockoutEnd = user.LockoutEnd
                });
            }

            // Filtering
            if (!string.IsNullOrEmpty(UserName))
            {
                result = result
                    .Where(u => u.userName.Contains(UserName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                result = result
                    .Where(u => u.RoleName == roleName)
                    .ToList();
            }

            // Pagination
            int pageSize = 25;
            int totalItems = result.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page = page < 1 ? 1 : page;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var usersList = result
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Roles dropdown
            var Allroles = await _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToListAsync();

            return View(new UsersVM()
            {
                UsersWitRoles = usersList,
                Roles = Allroles,
                PageCount = totalPages,
                CurrentPage = page
            });
        }



        public async Task<IActionResult> Edit(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRole = userRoles.FirstOrDefault();
            var allRoles = await _context.Roles.ToListAsync();

            var model = new UserVM
            {
                id = user.Id,
                Email = user.Email,
                userName = user.UserName,
                IsConfirmed = user.EmailConfirmed,
                RoleName = currentRole,
                IsLocked = user.LockoutEnd > DateTime.UtcNow,
                LockoutEnd = user.LockoutEnd,

                Roles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = r.Name == currentRole
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromRoute] string id, UserVM vm)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.Email = vm.Email;
            user.UserName = vm.userName;
            user.EmailConfirmed = vm.IsConfirmed;
            user.LockoutEnabled= vm.IsLocked;
            user.LockoutEnd = vm.LockoutEnd;

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles is not null)
                if (roles.Any())
                    await _userManager.RemoveFromRolesAsync(user, roles);
            if (!string.IsNullOrEmpty(vm.RoleName))
                await _userManager.AddToRoleAsync(user, vm.RoleName);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            await _userManager.DeleteAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles is not null)
                if (roles.Any())
                    await _userManager.RemoveFromRolesAsync(user, roles);

            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            var allRoles = await _context.Roles.ToListAsync();

            var model = new UserCreateVM
            {
                Roles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList()
            };

            return View(model);

        }

        private async Task<UserCreateVM> ReloadUserCreateVMAsync()
        {
            ModelState.Remove(key: "FirstName");
            ModelState.Remove("LastName");
            var allRoles = await _context.Roles.ToListAsync();

            return new UserCreateVM
            {
                Roles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList()
            };
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateVM model)
        {
            if (!ModelState.IsValid)
            { 
           
                model = await ReloadUserCreateVMAsync();
                 return View(model);


            }


            var user = new ApplicationUser
            {
                UserName = model.userName,
                Email = model.Email,
                EmailConfirmed = model.IsConfirmed
                ,
                FirstName = model.userName,
                LastName = model.userName,

            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                model = await ReloadUserCreateVMAsync();

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }


            // Assign role
            if (!string.IsNullOrEmpty(model.RoleName))
                await _userManager.AddToRoleAsync(user, model.RoleName);

            return RedirectToAction(nameof(Index));


        }
 





        }
}
