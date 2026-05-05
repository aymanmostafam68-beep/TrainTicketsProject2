using Microsoft.AspNetCore.Mvc;

namespace TrainTicketsProject.Areas.Admin.Controllers
{
    [Area(AreaRoles.AdminArea)]

    public class ReportController : Controller
    {
        private IRepository<Carriage> _Carriagerepository;
        private IRepository<Train> _Trainrepository;
        private IRepository<TrainClass> _trainClassRepository;

        public ReportController(IRepository<Carriage> carriagerepository, IRepository<Train> trainrepository, IRepository<TrainClass> trainClassRepository)
        {
            _Carriagerepository = carriagerepository;
            _Trainrepository = trainrepository;
            _trainClassRepository = trainClassRepository;
        }

        public async Task<IActionResult> Index(string? TrainCode, int page = 1)
        {
            var TrainReport = await _Trainrepository.GetAll(null, (items => items.Include(r=>r.Route).Include(i => i.Carriages)
            .ThenInclude(c => c.CarriageSeats)

            ), tracked: false);
            var query = TrainReport

          .Select(t => new TrainSummaryVM
          {
              id = t.TrainId,
              TrainCode = t.TrainCode,
              TrainName = t.TrainName,
              CarriageCount = t.Carriages.Count(),
              Capacity = t.Carriages.Sum(c => c.Capacity),
              AvailableSeats = t.Carriages.Sum(c => c.CarriageSeats.Count(cs => cs.IsAvailable)),
              Route = t.Route.RouteName,
              IsActive = t.IsActive

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




            return View(new TrainReportVm
            {
                trains = query.AsEnumerable(),
                CurrentPage = page,
                TotalPages = totalPages
            }

            );
        }


        [HttpGet]
        public async Task<IActionResult> TrainInfo([FromRoute] int id)
{
            var trains = await _Trainrepository.GetAll(
                t => t.TrainId == id,
                items => items
                    .Include(t => t.Carriages)
                        .ThenInclude(c => c.CarriageSeats)
                        ,
                tracked: false
            );

            var train = trains.FirstOrDefault();

    if (train == null)
        return NotFound();

    var model = new TrainInfoVM
    {
        TrainId= train.TrainId,
        TrainCode = train.TrainCode,
        TrainName = train.TrainName,
        Carriages = train.Carriages.Select(c => new CarriageInfoVM 
        {
            CarriageName = c.CarriageName,
            Capacity = c.Capacity,

            // assuming seat has IsReserved or similar
            AvailableSeats = c.CarriageSeats.Count(s => s.IsAvailable),
            CreatedAt = c.CreatedAt,
            TrainClass = _trainClassRepository.GetOne(tc => tc.Id == c.TrainClassId).Result.Name ?? "Unknown"
        }).ToList()
    };

    return View(model);
}


        //public async Task<IActionResult> ExportToExcel()
        //{
        //    var TrainReport = await _Trainrepository.GetAll(null, (items => items.Include(r => r.Route).Include(i => i.Carriages)
        //    .ThenInclude(c => c.CarriageSeats)
        //    ), tracked: false);
        //    var query = TrainReport
        //  .Select(t => new TrainSummaryVM
        //  {
        //      Id = t.TrainId,
        //      TrainCode = t.TrainCode,
        //      TrainName = t.TrainName,
        //      CarriageCount = t.Carriages.Count(),
        //      Capacity = t.Carriages.Sum(c => c.Capacity),
        //      AvailableSeats = t.Carriages.Sum(c => c.CarriageSeats.Count(cs => cs.IsAvailable)),
        //      Route = t.Route.RouteName,
        //      IsActive = t.IsActive
        //  }).ToList();
        //    var stream = ExcelExporter.ExportToExcel(query);
        //    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TrainReport.xlsx");

    }
}
