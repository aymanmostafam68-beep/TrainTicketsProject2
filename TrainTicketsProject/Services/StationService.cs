using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TrainTicketsProject.Interfaces; 
using TrainTicketsProject.Models;
using TrainTicketsProject.Services; 

namespace TrainTicketsProject.Services
{

    public class StationService : IStationService
    {


        private readonly IRepository<Station> _stationRepo;

        public StationService(IRepository<Station> stationRepo)
        {
            _stationRepo = stationRepo;
        }


        public async Task<List<Station>> GetAllStationsAsync()
        {
           return await _stationRepo.GetAll(c=>c.IsActive);
        }

        public async Task<Station?> GetStationByCodeAsync(string code)
        {
            return await _stationRepo.GetOne(c => c.Code == code);
        }



        // create station
        public async Task<bool> CreateStationAsync(Station station)
        {


            //1- check if station code is exist

            var existing = await GetStationByCodeAsync(station.Code);

            if (existing != null)
            {

                return false;


            }
           
         //2- if station code is new

                await _stationRepo.Create(station);

        //3- save new station in db
              
           

            var result = await _stationRepo.Comment();

            
            return result > 0;



        }

        //update station

        public async Task<Station?> UpdateStationAsync(Station station)
        {
            // 1. Ensure that the station exists in the database

            var existingStation = await _stationRepo.GetOne(s => s.StationId == station.StationId);

            if (existingStation == null)
            {
                return null;
            }

            // 2.Update only required fields

            existingStation.Name = station.Name;
            existingStation.Description = station.Description;
            existingStation.IsActive = station.IsActive;

           
            await _stationRepo.update(existingStation);
            var result = await _stationRepo.Comment();

            return result > 0 ? existingStation : null;
        }


        //delete station

        public async Task<bool> DeleteStationAsync(int id)
        {

            var station = await _stationRepo.GetOne(c => c.StationId == id);

            if (station == null || station.IsActive)
            {

                return false;

            }


            await _stationRepo.Delete(station);

            var result = await _stationRepo.Comment();

            return result > 0;

        }

    }
}
