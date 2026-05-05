using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainTicketsProject.Models;
using TrainTicketsProject.Models.ViewsModel.CarriageVMs;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class CarraiageSeatController : Controller
    {
        private IRepository<Train> _trainrepository;
        private IRepository<Carriage> _carriagerepository;
        private IRepository<CarriageSeat> _carriageseatrepository;

        public CarraiageSeatController(IRepository<Train> trainrepository, IRepository<Carriage> carriagerepository, IRepository<CarriageSeat> carriageseatrepository)
        {
            _trainrepository = trainrepository;
            _carriagerepository = carriagerepository;
            _carriageseatrepository = carriageseatrepository;
        }

        public async Task<IActionResult> Index(string? TrainCode, int page = 1)
        {
            var trainsCarriage = await _trainrepository.GetAll(null, (items => items.Include(i => i.Carriages)
            .ThenInclude(c => c.CarriageSeats)

            ), tracked: false);
            var query = trainsCarriage

          .Select(t => new TrainSummaryVM
               {
              id= t.TrainId,
              TrainCode = t.TrainCode,
                   TrainName = t.TrainName,
                   CarriageCount = t.Carriages.Count(),
                   Capacity = t.Carriages.Sum(c => c.Capacity),
                   AvailableSeats = t.Carriages.Sum(c => c.CarriageSeats.Count(cs => cs.IsAvailable))
               }).ToList();
            //Columns: train/ count carriage capacity / generate seats for carriage

            if (TrainCode is not null)
            {
                query = query.Where(e => e.TrainCode.Contains(TrainCode)).ToList();
            }
            int totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / 12.0);
            page = page < 1 ? 1 : page;
            page = page > totalPages ? 1 : page;


            query = query
                .Skip((page - 1) * 12)
                .Take(12).ToList();




            return View(new CarriageSeatVM
            {
                trains = query.AsEnumerable(),
                CurrentPage = page,
                TotalPages = totalPages
            }

            );
        }

        public async Task<IActionResult> Edit([FromRoute] int id)
        {

            var trains = await _trainrepository.GetAll(
                          t => t.TrainId == id,
                          items => items
                              .Include(t => t.Carriages)
                                  .ThenInclude(c => c.CarriageSeats)
                               .Include(t => t.Carriages) 
                                .ThenInclude(c => c.TrainClass)



                          , tracked: false
                      );

            var train = trains.FirstOrDefault();

            if (train == null)
                return NotFound();



            var model = new TrainInfoVM
            {
                TrainId = train.TrainId,
                TrainCode = train.TrainCode,
                TrainName = train.TrainName,
                Carriages = train.Carriages.Select(c => new CarriageInfoVM
                {
                    CarriageId = c.CarriageId,
                    CarriageName = c.CarriageName,
                    Capacity = c.Capacity,
                    CreatedSeats = c.CarriageSeats.Count(s => s.IsAvailable),
                    TrainClass = c.TrainClass.Name ?? "Unknown"
                }).ToList()



            };

            return View(model);

        }



        [HttpPost]
        public async Task<IActionResult>  SeatsGenerator(TrainInfoVM model)
        {
            if (model.TrainId == 0) return NotFound();

           

            var trains = await _trainrepository.GetAll(
                t => t.TrainId == model.TrainId,
                items => items.Include(t => t.Carriages).ThenInclude(c => c.CarriageSeats),
                tracked: false);

            var train = trains.FirstOrDefault();
            if (train == null) return NotFound();

            var seatsToAdd = new List<CarriageSeat>();

            foreach (var carriage in train.Carriages)
            {
                int currentSeatCount = carriage.CarriageSeats?.Count() ?? 0;

                // If we have fewer seats than the capacity, add the difference
                if (currentSeatCount < carriage.Capacity)
                {
                    // Start loop from the next available seat number
                    for (int i = currentSeatCount + 1; i <= carriage.Capacity; i++)
                    {
                        seatsToAdd.Add(new CarriageSeat
                        {
                            SeatNumber = i,
                            IsAvailable = true,
                            CarriageId = carriage.CarriageId
                        });
                    }
                }
            }

            if (seatsToAdd.Any())
            {
                await _carriageseatrepository.AddRange(seatsToAdd);
                TempData["Notification-success"] = "Seats added successfully";

                await _carriageseatrepository.Comment();
            }
            else
            {
                TempData["Notification-error"] = "No seats were added";

            }


            return RedirectToAction(nameof(Edit), new { id = model.TrainId });
        }


        [HttpPost]

        public async Task<IActionResult> GenerateSeats([FromRoute]int id) 
        {
            if (id == 0) return NotFound();


         var carriage = await _carriagerepository.GetOne(
        c => c.CarriageId == id,
        s => s.Include(c => c.CarriageSeats),
        tracked: true);

            if (carriage == null) return NotFound();

            int currentSeatCount = carriage.CarriageSeats?.Count ?? 0;
            var seatsToAdd = new List<CarriageSeat>();

            if (currentSeatCount < carriage.Capacity)
            {
                for (int i = currentSeatCount + 1; i <= carriage.Capacity; i++)
                {
                    seatsToAdd.Add(new CarriageSeat
                    {
                        SeatNumber = i,
                        IsAvailable = true,
                        CarriageId = id
                    });
                }
            }

            if (seatsToAdd.Any())
            {
                await _carriageseatrepository.AddRange(seatsToAdd);
              int result =  await _carriageseatrepository.Comment();

                if (result > 0)
                {
                    TempData["Notification-success"] = "Seats added successfully";
                }
                else
                {
                    TempData["Notification-error"] = "Error adding Carriage Seats";
                }

            }


            return RedirectToAction("Index", new { id = carriage.TrainId});
        }


        [HttpGet]
        public async Task<IActionResult> ViewSeats([FromRoute] int id)
        {
            var carriage = await _carriagerepository.GetOne(
  c => c.CarriageId == id,
  s => s.Include(c => c.CarriageSeats),
  tracked: true);

            if (carriage == null) return NotFound();


            var model = new ViewSeatsInfoVM()
            {
                CarriageId = id,
                CarriageName = carriage.CarriageName,
                Capacity = carriage.Capacity,
                carriageSeats = carriage.CarriageSeats.Select(c => new CarriageSeat
                {
                    CarriageSeatId = c.CarriageSeatId,
                    CarriageId = c.CarriageId,
                    SeatNumber = c.SeatNumber,
                    IsAvailable = c.IsAvailable,
                    IsOutOfService = c.IsOutOfService

                }).ToList()
            };

            return View(model);


        }

        [HttpPost]
        public async Task<IActionResult> IsOutOfService([FromRoute] int id, int carriageId)
        {
            if (id == 0 || carriageId == 0) return NotFound();

            var seat = await _carriageseatrepository.GetOne(
                s => s.CarriageSeatId == id && s.CarriageId == carriageId,
                tracked: true);

            if (seat == null) return NotFound();

            seat.IsOutOfService = !seat.IsOutOfService;

            await _carriageseatrepository.update(seat);
            int result = await _carriageseatrepository.Comment();

            if (result > 0)
            {
                TempData["Notification-success"] = seat.IsOutOfService
                    ? "Seat marked as out of service"
                    : "Seat returned to service";
            }
            else
            {
                TempData["Notification-error"] = "Error updating seat status";
            }

            return RedirectToAction(nameof(ViewSeats), new { id = carriageId });
        }



    }
}
