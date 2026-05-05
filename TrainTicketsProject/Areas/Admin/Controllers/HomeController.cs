using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using TrainTicketsProject.ApplicationDB;
using TrainTicketsProject.ViewModels;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]
    [Authorize(Roles = $"{AreaRoles.Admin_Role},{AreaRoles.Super_Admin_Role}")]
    public class HomeController : Controller
    {

        private readonly ApplicationDataAccess _context;


        public HomeController(ApplicationDataAccess context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // ========= Cards =========
            var totalTrips = _context.trips.Count();
            var totalBookings = _context.bookings.Count();
            var totalTrains = _context.trains.Count();
            var totalTransactions = _context.transactions.Count();
            var totalSeats = _context.carriageSeats.Count();

            double occupancyRate = 0;

            if (totalSeats > 0 && totalTrips > 0)
            {
                occupancyRate = (double)totalBookings / (totalSeats * totalTrips) * 100;
            }

            // ========= Chart 1: Bookings per Day =========
            var bookingsPerDay = _context.bookings
                .GroupBy(b => b.TripScheduleId)
                .Select(g => new
                {
                    Date = _context.trips
                        .Where(t => t.TripScheduleId == g.Key)
                        .Select(t => t.TravelDate.Date)
                        .FirstOrDefault(),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.BookingsLabels = bookingsPerDay
                .Select(x => x.Date.ToString("yyyy-MM-dd"))
                .ToList();

            ViewBag.BookingsData = bookingsPerDay
                .Select(x => x.Count)
                .ToList();

            // ========= Chart 2: Revenue per Day =========
            var revenuePerDay = _context.transactions
                .Where(t => t.PaymentStatus == PaymentStatus.Completed)
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.RevenueLabels = revenuePerDay
                .Select(x => x.Date.ToString("yyyy-MM-dd"))
                .ToList();

            ViewBag.RevenueData = revenuePerDay
                .Select(x => x.Total)
                .ToList();

            // ========= Chart 3: Trips per Day =========
            var tripsPerDay = _context.trips
                .GroupBy(t => t.TravelDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.TripsLabels = tripsPerDay
                .Select(x => x.Date.ToString("yyyy-MM-dd"))
                .ToList();

            ViewBag.TripsData = tripsPerDay
                .Select(x => x.Count)
                .ToList();

            // ========= VM =========
            var vm = new DashboardVM
            {
                TotalTrips = totalTrips,
                TotalBookings = totalBookings,
                TotalTrains = totalTrains,
                TotalTransactions = totalTransactions,
                OccupancyRate = Math.Round(occupancyRate, 2)
            };

            return View(vm);
        }
    }
}
