using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using TrainTicketsProject.ApplicationDB;

namespace TrainTicketsProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDB.ApplicationDataAccess>
             (
             e => e.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
               );


            builder.Services.AddScoped<UserManager<ApplicationUser>, UserManager<ApplicationUser>>();


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>
                (e =>
                {
                    e.User.RequireUniqueEmail = true;
                    e.Password.RequiredLength = 8;
                    e.Lockout.MaxFailedAccessAttempts = 3;

                    e.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                }

                ).AddEntityFrameworkStores<ApplicationDB.ApplicationDataAccess>()
                .AddDefaultTokenProviders();


            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IRepository<Models.GeneralSetting>, Repository<Models.GeneralSetting>>();
            builder.Services.AddScoped<IRepository<Models.Train>, Repository<Models.Train>>();
            builder.Services.AddScoped<IRepository<Models.Carriage>, Repository<Models.Carriage>>();
            builder.Services.AddScoped<IRepository<Models.TrainClass>, Repository<Models.TrainClass>>();

            builder.Services.AddScoped<IRepository<Models.CarriageSeat>, Repository<Models.CarriageSeat>>();
            builder.Services.AddScoped<IRepository<Models.Booking>, Repository<Models.Booking>>();
            builder.Services.AddScoped<IRepository<Models.Transaction>, Repository<Models.Transaction>>();
            builder.Services.AddScoped<IRepository<Models.TransactionEntry>, Repository<Models.TransactionEntry>>();
            builder.Services.AddScoped<IRepository<Models.TripSchedule>, Repository<Models.TripSchedule>>();
            builder.Services.AddScoped<IRepository<Models.Trip>, Repository<Models.Trip>>(); 
            builder.Services.AddScoped<IRepository<Models.Station>, Repository<Models.Station>>();
            builder.Services.AddScoped<IRepository<Models.Route>, Repository<Models.Route>>();
            builder.Services.AddScoped<IRepository<Models.DepartureTimeInterval>, Repository<Models.DepartureTimeInterval>>();



            builder.Services.AddScoped<IRepository<Models.RouteStation>, Repository<Models.RouteStation>>();

            builder.Services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();

            builder.Services.AddScoped<ApplicationDB.ApplicationDataAccess, ApplicationDB.ApplicationDataAccess>();


            //Service
            builder.Services.AddScoped<ITripScheduleService, TripScheduleService>();
            builder.Services.AddScoped<ITripServiceIndex, TripServiceIndex>();
            builder.Services.AddScoped<ITimeInterval, TimeInterval>();

            builder.Services.AddScoped<IAccountService, AccountService>();

            builder.Services.AddScoped<IStationService, StationService>();
            builder.Services.AddScoped<ICarriageService, CarriageService>();
            builder.Services.AddScoped<ICarriageClassService, CarriageClassService>();
            builder.Services.AddScoped<ICarraiageSeatService, CarraiageSeatService>();

            builder.Services.AddScoped<IRouteService, RouteService>();
            builder.Services.AddScoped<IGetUserInfoEntity,GetUserInfoEntity>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddScoped<IDbInitialize, DbInitialize>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();


            // External Authentication - Google
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    // Load credentials from appsettings.json
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                }).AddFacebook(facebookOptions =>
                {
                    facebookOptions.ClientId = builder.Configuration["Authentication:Facebook:ClientId"]!;
                    facebookOptions.ClientSecret = builder.Configuration["Authentication:Facebook:ClientSecret"]!;
                });

            //builder.Services.AddControllersWithViews(options =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        .Build();

            //    options.Filters.Add(new AuthorizeFilter(policy));
            //});

            //builder.Services.AddAuthentication("Cookies")
            //.AddCookie("Cookies", options =>
            //{
            //    options.Events = new CookieAuthenticationEvents
            //    {
            //        OnRedirectToLogin = context =>
            //        {
            //            var path = context.Request.Path.Value.ToLower();

            //            if (path.StartsWith("/admin"))
            //            {
            //                context.Response.Redirect("/Admin/Account/Login");
            //            }
            //            else if (path.StartsWith("/customer"))
            //            {
            //                context.Response.Redirect("/Customer/Account/Login");
            //            }
            //            else
            //            {
            //                context.Response.Redirect("/Account/Login"); // fallback
            //            }

            //            return Task.CompletedTask;
            //        }
            //    };
            //});




            var app = builder.Build();

             // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            using (var scope = app.Services.CreateScope())
            {
                var dbInitialize = scope.ServiceProvider.GetRequiredService<IDbInitialize>();
               await dbInitialize.Initialize();
            }



            app.UseHttpsRedirection();
            app.MapStaticAssets();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Identity}/{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
