using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace TrainTicketsProject.Services
{
    public class TripScheduleService : ITripScheduleService
    {
        public readonly IRepository<TripSchedule> _tripScheduleRepository;
        public readonly IRepository<Route> _routeRepository;
        public readonly IRepository<Train> _trainRepository;
        public readonly IRepository<Trip> _tripRepository;
        public readonly ITimeInterval _intervalServices;

        public TripScheduleService(
            IRepository<TripSchedule> tripScheduleRepository,
            IRepository<Route> routeRepository,
            IRepository<Train> trainRepository,
            IRepository<Trip> tripRepository,
            ITimeInterval intervalServices)
        {
            _tripScheduleRepository = tripScheduleRepository;
            _routeRepository = routeRepository;
            _trainRepository = trainRepository;
            _tripRepository = tripRepository;
            _intervalServices = intervalServices;
        }

        public async Task<TripSchedulesVM> GetIndexVmAsync(int page)
        {
            var tripSchedules = await _tripScheduleRepository.GetAll(
                s => !s.IsReturnTrip,
                q => q.Include(s => s.Route)
                      .Include(s => s.Train)
                      .Include(s => s.ReturnSchedule),
                tracked: false);

            int pageSize = 12;

            var totalPages = Math.Ceiling((double)tripSchedules.Count / pageSize);

            tripSchedules = tripSchedules
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new TripSchedulesVM
            {
                TripSchedule = tripSchedules,
                Route = await _routeRepository.GetAll(tracked: false),
                Train = await _trainRepository.GetAll(tracked: false),
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<TripScheduleCreateVM> GetCreateVmAsync()
        {
            var vm = new TripScheduleCreateVM
            {
                TripSchedule = new TripSchedule
                {
                    StartDay = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(2),
                    Every = 1
                }
            };

            await PopulateVmLists(vm);

            return vm;
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(
            TripScheduleCreateVM model,
            string userId,
            ModelStateDictionary modelState)
        {
            RemoveModelState(modelState);

            var selectedTrain = await GetSelectedTrainAsync(
                model.TripSchedule.TrainId);

            if (selectedTrain == null)
            {
                modelState.AddModelError(
                    "TripSchedule.TrainId","Please select a valid train.");

                await PopulateCreateVmLists(model);

                return (false, "Please select a valid train");
            }

            ApplyAssignedRoute(model, selectedTrain);

            if (!TryApplySelectedTimes(model, modelState)) 
            {
                await PopulateCreateVmLists(model);
                return (false, "Please select a valid Time.");
            }
            TripSchedule? preparedReturnSchedule = null;

            if (model.HasReturnTrip)
            {
                preparedReturnSchedule =
                    await PrepareReturnScheduleAsync(
                        model,
                        selectedTrain,
                        userId,
                        null,
                        modelState);

                if (preparedReturnSchedule == null)
                {
                    await PopulateCreateVmLists(model);

                    return (false, "Please select a valid train");
                }
            }

            model.TripSchedule.CreatedUserId = userId;
            model.TripSchedule.UpdatedUserId = userId;

            ApplyComputedFields(model.TripSchedule);

            await _tripScheduleRepository.Create(model.TripSchedule);

            var result = await _tripScheduleRepository.Comment();

            if (result <= 0)
            {
                return (false, "Error creating trip schedule.");
           
            }

            await SyncTripsAsync(model.TripSchedule);

            if (preparedReturnSchedule != null)
            {
                var returnSchedule =
                    await SavePreparedReturnScheduleAsync(
                        preparedReturnSchedule,
                        modelState);

                if (returnSchedule == null)
                {
                    return (false,
                        "Error saving return trip.");
                }

                model.TripSchedule.ReturnScheduleId =
                    returnSchedule.TripScheduleId;

                await _tripScheduleRepository.update(
                    model.TripSchedule);

                await _tripScheduleRepository.Comment();
            }

            return (true,"The trip Created Successfully");
        }

        public async Task<TripScheduleCreateVM?> GetEditVmAsync(int id)
        {
            var schedule = await _tripScheduleRepository.GetOne(
                s => s.TripScheduleId == id,
                q => q.Include(s => s.Route)
                      .Include(s => s.Train),
                tracked: false);

            if (schedule == null)
            {
                return null;
            }

            var vm = new TripScheduleCreateVM
            {
                TripSchedule = schedule,
                SelectedDepartureTime = schedule.DepartureTime.ToString(@"HH\:mm"),
                SelectedArrivalTime = schedule.ArrivalTime.ToString(@"HH\:mm")
            };

            if (schedule.ReturnScheduleId.HasValue)
            {
                var returnSchedule = await _tripScheduleRepository.GetOne(
                    s => s.TripScheduleId == schedule.ReturnScheduleId.Value,
                    tracked: false);
                if (returnSchedule != null)
                {
                    vm.HasReturnTrip = true;
                    vm.ReturnTripScheduleId = returnSchedule.TripScheduleId;
                    vm.ReturnSelectedDepartureTime = returnSchedule.DepartureTime.ToString(@"HH\:mm");
                    vm.ReturnSelectedArrivalTime = returnSchedule.ArrivalTime.ToString(@"HH\:mm");
                    vm.ReturnIsNextDay = returnSchedule.IsNextDay;
                    vm.ReturnStartDay = returnSchedule.StartDay;
                    vm.ReturnEndDate = returnSchedule.EndDate;
                    vm.ReturnEvery = returnSchedule.Every;
                }
            }


            await PopulateVmLists(vm);

            return vm;
        }






        public async Task<(bool Success, string? Error)> EditAsync(
            TripScheduleCreateVM model,
            string userId,
            ModelStateDictionary modelState)
        {
            var existingSchedule = await _tripScheduleRepository.GetOne(
                s => s.TripScheduleId == model.TripSchedule.TripScheduleId,
                tracked: true);

            if (existingSchedule == null)
            {
                return (false, "Schedule not found.");
            }

            var selectedTrain = await GetSelectedTrainAsync(model.TripSchedule.TrainId);
            if (selectedTrain == null)
            {
                modelState.AddModelError("TripSchedule.TrainId", "Please select a valid train.");
                await PopulateCreateVmLists(model);
                return (false, "Please select a valid train.");
            }
            ApplyAssignedRoute(model, selectedTrain);

            if (!TryApplySelectedTimes(model,modelState) || !ValidateScheduleRange(model.TripSchedule, "TripSchedule.EndDate", "TripSchedule.Every",modelState))
            {
                await PopulateCreateVmLists(model);
                return (false, "Please select a valid Time.");
            }


            TripSchedule? preparedReturnSchedule = null;
            if (model.HasReturnTrip)
            {
                preparedReturnSchedule = await PrepareReturnScheduleAsync(model, selectedTrain, userId, existingSchedule.ReturnScheduleId,modelState);
                if (preparedReturnSchedule == null)
                {
                    await PopulateCreateVmLists(model);
                    return (false, string.Empty);
                }
            }

            existingSchedule.RouteId = selectedTrain.RouteId;
            existingSchedule.TrainId = selectedTrain.TrainId;
            existingSchedule.DepartureTime = model.TripSchedule.DepartureTime;
            existingSchedule.ArrivalTime = model.TripSchedule.ArrivalTime;
            existingSchedule.IsNextDay = model.TripSchedule.IsNextDay;
            existingSchedule.StartDay = model.TripSchedule.StartDay;
            existingSchedule.EndDate = model.TripSchedule.EndDate;
            existingSchedule.Every = model.TripSchedule.Every;
            existingSchedule.IsActive = model.TripSchedule.IsActive;
            existingSchedule.UpdatedUserId = userId;
            existingSchedule.IsReturnTrip = false;
            ApplyComputedFields(existingSchedule);

            await _tripScheduleRepository.update(existingSchedule);
            var result = await _tripScheduleRepository.Comment();

            if (result <= 0)
            {
                await PopulateCreateVmLists(model);
                return (false, "Error updating trip schedule");
            }

            await SyncTripsAsync(existingSchedule);

            if (model.HasReturnTrip)
            {
                var returnSchedule = await SavePreparedReturnScheduleAsync(preparedReturnSchedule!,modelState);
                if (returnSchedule == null)
                {
                    await PopulateCreateVmLists(model);
                    return (false, string.Empty);
                }

                if (existingSchedule.ReturnScheduleId != returnSchedule.TripScheduleId)
                {
                    existingSchedule.ReturnScheduleId = returnSchedule.TripScheduleId;
                    await _tripScheduleRepository.update(existingSchedule);
                    await _tripScheduleRepository.Comment();
                }
            }
            else if (existingSchedule.ReturnScheduleId.HasValue)
            {
                await DeleteLinkedReturnScheduleAsync(existingSchedule.ReturnScheduleId.Value);
                existingSchedule.ReturnScheduleId = null;
                await _tripScheduleRepository.update(existingSchedule);
                await _tripScheduleRepository.Comment();
            }


            return (true, "Trip schedule updated successfully.");
                    

        }

        public async Task<bool> DeleteAsync(int id)
        {
            var schedule = await _tripScheduleRepository.GetOne(
                s => s.TripScheduleId == id,
                tracked: true);

            if (schedule == null)
            {
                return false;
            }

            await DeleteTripsAsync(id);

            await _tripScheduleRepository.Delete(schedule);

            await _tripScheduleRepository.Comment();

            return true;
        }

        public async Task PopulateVmLists(TripScheduleCreateVM vm)
        {
            var intervals = await _intervalServices.GetAllTimeIntervals();

            vm.Route = await _routeRepository.GetAll(tracked: false);

            vm.Train = await _trainRepository.GetAll(
                null,
                q => q.Include(t => t.Route),
                tracked: false);

            vm.TimeSlots = intervals
                .Select(i => new SelectListItem
                {
                    Value = i.DepartureTime.ToString(@"hh\:mm"),
                    Text = i.DepartureTime.ToString(@"hh\:mm")
                }).ToList().OrderBy(c=>c.Text);
        }

        public void ApplyComputedFields(TripSchedule schedule)
        {
            schedule.TimeSlot =
                $"{schedule.DepartureTime:hh\\:mm} - {schedule.ArrivalTime:hh\\:mm}";
            schedule.Duration = CalculateDuration(schedule.DepartureTime, schedule.ArrivalTime, schedule.IsNextDay);

        }

        public async Task SyncTripsAsync(TripSchedule schedule)
        {
            await DeleteTripsAsync(schedule.TripScheduleId);

            var trips = new List<Trip>();

            var current = schedule.StartDay;

            while (current <= schedule.EndDate)
            {
                trips.Add(new Trip
                {
                    TripScheduleId = schedule.TripScheduleId,
                    TravelDate = current,
                    status = schedule.IsActive
                });

                current = current.AddDays(schedule.Every);
            }

            if (trips.Any())
            {
                await _tripRepository.AddRange(trips);
                await _tripRepository.Comment();
            }
        }

        public async Task DeleteTripsAsync(int tripScheduleId)
        {
            var trips = await _tripRepository.GetAll(
                t => t.TripScheduleId == tripScheduleId,
                tracked: true);

            if (trips.Any())
            {
                await _tripRepository.DeleteRange(trips);
                await _tripRepository.Comment();
            }
        }



        public void RemoveModelState(ModelStateDictionary ModelState)
        {
            ModelState.Remove("TripSchedule.Route");
            ModelState.Remove("TripSchedule.Train");
            ModelState.Remove("TripSchedule.ReturnSchedule");
            ModelState.Remove("Route");
            ModelState.Remove("Train");
            ModelState.Remove("TimeSlots");
        }

        public async Task PopulateCreateVmLists(TripScheduleCreateVM vm)
        {
            var intervals = await _intervalServices.GetAllTimeIntervals();
            var routes = await _routeRepository.GetAll(tracked: false);
            var trains = await _trainRepository.GetAll(
                null,
                q => q.Include(t => t.Route),
                tracked: false);

            vm.Route = routes;
            vm.Train = trains;
            vm.TimeSlots = intervals
                .Select(i => new SelectListItem
                {
                    Value = i.DepartureTime.ToString(@"hh\:mm"),
                    Text = i.DepartureTime.ToString(@"hh\:mm")
                })
                .OrderBy(i => i.Text)
                .ToList().OrderBy(c=>c.Text);

            if (string.IsNullOrWhiteSpace(vm.AssignedRouteName) && vm.TripSchedule.TrainId > 0)
            {
                vm.AssignedRouteName = trains.FirstOrDefault(t => t.TrainId == vm.TripSchedule.TrainId)?.Route?.RouteName ?? string.Empty;
            }
        }

        public async Task<Train?> GetSelectedTrainAsync(int trainId)
        {
            if (trainId == 0)
            {
                return null;
            }

            return await _trainRepository.GetOne(
                t => t.TrainId == trainId,
                q => q.Include(t => t.Route),
                tracked: false);
        }

        public void ApplyAssignedRoute(TripScheduleCreateVM model, Train selectedTrain)
        {
            model.TripSchedule.RouteId = selectedTrain.RouteId;
            model.AssignedRouteName = selectedTrain.Route?.RouteName ?? string.Empty;
        }


        public bool TryApplySelectedTimes(TripScheduleCreateVM model,ModelStateDictionary modestate )
        {
            if (!TryParseTimes(
                model.SelectedDepartureTime,
                model.SelectedArrivalTime,
                model.TripSchedule.IsNextDay,
                "SelectedDepartureTime",
                "SelectedArrivalTime",
                out var departureTime,
                out var arrivalTime, modestate))
            {
                return false;
            }

            model.TripSchedule.DepartureTime = departureTime;
            model.TripSchedule.ArrivalTime = arrivalTime;
            return true;
        }

        public bool TryApplyReturnTimes(
            TripScheduleCreateVM model,
            out TimeOnly departureTime,
            out TimeOnly arrivalTime,
ModelStateDictionary modelState) 
        {
            return TryParseTimes(
                model.ReturnSelectedDepartureTime,
                model.ReturnSelectedArrivalTime,
                model.ReturnIsNextDay,
                "ReturnSelectedDepartureTime",
                "ReturnSelectedArrivalTime",
                out departureTime,
                out arrivalTime, modelState);
        }

        public bool TryParseTimes(
            string? selectedDepartureTime,
            string? selectedArrivalTime,
            bool isNextDay,
            string departureKey,
            string arrivalKey,
            out TimeOnly departureTime,
            out TimeOnly arrivalTime, ModelStateDictionary modelstate)
        {
            departureTime = default;
            arrivalTime = default;

            if (string.IsNullOrWhiteSpace(selectedDepartureTime))
            {
                modelstate.AddModelError(departureKey, "Please select departure time.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(selectedArrivalTime))
            {
                modelstate.AddModelError(arrivalKey, "Please select arrival time.");
                return false;
            }

            if (!TimeSpan.TryParse(selectedDepartureTime, out var departureSpan))
            {
                modelstate.AddModelError(departureKey, "Invalid departure time.");
                return false;
            }

            if (!TimeSpan.TryParse(selectedArrivalTime, out var arrivalSpan))
            {
                modelstate.AddModelError(arrivalKey, "Invalid arrival time.");
                return false;
            }

            if (arrivalSpan <= departureSpan && !isNextDay)
            {
                modelstate.AddModelError(arrivalKey, "Arrival time must be after departure time, or mark as next day.");
                return false;
            }

            departureTime = TimeOnly.FromTimeSpan(departureSpan);
            arrivalTime = TimeOnly.FromTimeSpan(arrivalSpan);
            return true;
        }

        public bool ValidateScheduleRange(TripSchedule schedule, string endDateKey, string everyKey, ModelStateDictionary modelstate)
        {
            if (schedule.EndDate < schedule.StartDay)
            {
                modelstate.AddModelError(endDateKey, "End date must be on or after start date.");
                return false;
            }

            if (schedule.Every <= 0)
            {
                modelstate.AddModelError(everyKey, "Repeat every must be greater than zero.");
                return false;
            }

            return true;
        }

    

        public TimeSpan CalculateDuration(TimeOnly departureTime, TimeOnly arrivalTime, bool isNextDay)
        {
            var departure = departureTime.ToTimeSpan();
            var arrival = arrivalTime.ToTimeSpan();

            if (isNextDay || arrival < departure)
            {
                arrival = arrival.Add(TimeSpan.FromDays(1));
            }

            return arrival - departure;
        }

        public async Task<bool> ScheduleExistsAsync(int routeId, int trainId, TimeOnly departureTime, int? ignoreScheduleId)
        {
            var existing = await _tripScheduleRepository.GetOne(
                s => s.RouteId == routeId
                     && s.TrainId == trainId
                     && s.DepartureTime == departureTime
                     && (!ignoreScheduleId.HasValue || s.TripScheduleId != ignoreScheduleId.Value));

            return existing != null;
        }

        public async Task<TripSchedule?> PrepareReturnScheduleAsync(
            TripScheduleCreateVM model,
            Train selectedTrain,
            string userId,
            int? existingReturnScheduleId,
            ModelStateDictionary modelstate)
        {
            if (!TryApplyReturnTimes(model, out var returnDeparture, out var returnArrival,modelstate))
            {
                return null;
            }

            var returnSchedule = existingReturnScheduleId.HasValue
                ? await _tripScheduleRepository.GetOne(s => s.TripScheduleId == existingReturnScheduleId.Value, tracked: true)
                : new TripSchedule();

            if (returnSchedule == null)
            {
                modelstate.AddModelError("", "The linked return trip could not be found.");
                return null;
            }

            returnSchedule.RouteId = selectedTrain.RouteId;
            returnSchedule.TrainId = selectedTrain.TrainId;
            returnSchedule.DepartureTime = returnDeparture;
            returnSchedule.ArrivalTime = returnArrival;
            returnSchedule.IsNextDay = model.ReturnIsNextDay;
            returnSchedule.StartDay = model.ReturnStartDay ?? model.TripSchedule.StartDay;
            returnSchedule.EndDate = model.ReturnEndDate ?? model.TripSchedule.EndDate;
            returnSchedule.Every = model.ReturnEvery ?? model.TripSchedule.Every;
            returnSchedule.IsActive = model.TripSchedule.IsActive;
            returnSchedule.IsReturnTrip = true;
            returnSchedule.UpdatedUserId = userId;

            if (!existingReturnScheduleId.HasValue)
            {
                returnSchedule.CreatedUserId = userId;
            }

            if (!ValidateScheduleRange(returnSchedule, "ReturnEndDate", "ReturnEvery", modelstate))
            {
                return null;
            }

            if (await ScheduleExistsAsync(returnSchedule.RouteId, returnSchedule.TrainId, returnSchedule.DepartureTime, existingReturnScheduleId))
            {
                modelstate.AddModelError("ReturnSelectedDepartureTime", "The return trip schedule already exists for this train and departure time.");
                return null;
            }

            ApplyComputedFields(returnSchedule);
            return returnSchedule;
        }

        public async Task<TripSchedule?> SavePreparedReturnScheduleAsync(TripSchedule returnSchedule ,ModelStateDictionary modelstate)
        {
            if (returnSchedule.TripScheduleId > 0)
            {
                await _tripScheduleRepository.update(returnSchedule);
            }
            else
            {
                await _tripScheduleRepository.Create(returnSchedule);
            }

            var result = await _tripScheduleRepository.Comment();
            if (result <= 0)
            {
                modelstate.AddModelError("", "Error saving the return trip schedule.");
                return null;
            }

            await SyncTripsAsync(returnSchedule);
            return returnSchedule;
        }


    

        public async Task DeleteLinkedReturnScheduleAsync(int returnScheduleId)
        {
            var returnSchedule = await _tripScheduleRepository.GetOne(s => s.TripScheduleId == returnScheduleId, tracked: true);
            if (returnSchedule == null)
            {
                return;
            }

            await DeleteTripsAsync(returnSchedule.TripScheduleId);
            await _tripScheduleRepository.Delete(returnSchedule);
            await _tripScheduleRepository.Comment();
        }
    }
}