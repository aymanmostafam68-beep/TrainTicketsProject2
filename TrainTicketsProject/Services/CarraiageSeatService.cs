using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Services
{
    public class CarraiageSeatService : ICarraiageSeatService
    {
        private IRepository<Train> _trainrepository;
        private IRepository<Carriage> _carriagerepository;
        private IRepository<CarriageSeat> _carriageseatrepository;

        public CarraiageSeatService(IRepository<Train> trainrepository, IRepository<Carriage> carriagerepository, IRepository<CarriageSeat> carriageseatrepository)
        {
            _trainrepository = trainrepository;
            _carriagerepository = carriagerepository;
            _carriageseatrepository = carriageseatrepository;
        }

        public async Task<CarriageSeatVM> SeatsVMIndex(string? TrainCode, int page = 1)
        {
            var trainsCarriage = await _trainrepository.GetAll(null, (items => items.Include(i => i.Carriages)
            .ThenInclude(c => c.CarriageSeats)

            ), tracked: false);
            var query = trainsCarriage

          .Select(t => new TrainSummaryVM
          {
              id = t.TrainId,
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




            return new  CarriageSeatVM
            {
                trains = query.AsEnumerable(),
                CurrentPage = page,
                TotalPages = totalPages
            };

        }


        public async Task<TrainInfoVM> EditPage([FromRoute] int id)
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
                return new TrainInfoVM();



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

            return model;

        }


        public async Task<(bool result,string? message)> SeatsGenerator(TrainInfoVM model)
        {
            if (model.TrainId == 0) return (false, "Invalid Train ID");

            var trains = await _trainrepository.GetAll(
                t => t.TrainId == model.TrainId,
                items => items.Include(t => t.Carriages).ThenInclude(c => c.CarriageSeats),
                tracked: false);

            var train = trains.FirstOrDefault();
            if (train == null) return (false, "Train not found");

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
                
                await _carriageseatrepository.Comment();
                return (true, "Seats added successfully");
            }
            else
            {
                return (false, "No seats were added");
            }
            }


        public async Task<(bool result, string? message, int trainId)> GenerateSeats([FromRoute] int id)
        {
            if (id == 0) return (false, "Invalid Carriage ID",0);


            var carriage = await _carriagerepository.GetOne(
           c => c.CarriageId == id,
           s => s.Include(c => c.CarriageSeats),
           tracked: true);

            if (carriage == null) return (false, "Carriage not found", 0);

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
                int result = await _carriageseatrepository.Comment();

                if (result == 0)
                 return (false, "Failed to add seats", 0 );

            }
            return (true, "Seats added successfully",carriage.TrainId);



        }




        public async Task<ViewSeatsInfoVM> ViewSeats([FromRoute] int id)
        {
            var carriage = await _carriagerepository.GetOne(
  c => c.CarriageId == id,
  s => s.Include(c => c.CarriageSeats),
  tracked: true);

            if (carriage == null) return new ViewSeatsInfoVM();


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

            return model;


        }


        public async Task<(bool result, string? message)> IsOutOfService([FromRoute] int id, int carriageId)
        {
            if (id == 0 || carriageId == 0) return (false,"Invalid seat or carriage ID");

            var seat = await _carriageseatrepository.GetOne(
                s => s.CarriageSeatId == id && s.CarriageId == carriageId,
                tracked: true);

            if (seat == null) return (false, "Seat not found");

            seat.IsOutOfService = !seat.IsOutOfService;

            await _carriageseatrepository.update(seat);
            int result = await _carriageseatrepository.Comment();

            if (result > 0)
            {
                return (true, seat.IsOutOfService ? "Seat marked as out of service" : "Seat returned to service");
            }
            else
            {
                return (false, "Error updating seat status");
            }

        }
    }
}
