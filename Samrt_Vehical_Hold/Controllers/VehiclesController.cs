using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Samrt_Vehical_Hold.DTO.VehicleHold;
using Samrt_Vehical_Hold.Entities;
using Samrt_Vehical_Hold.Helpers.Service;
using Samrt_Vehical_Hold.Repo.Interface;
using System.Drawing;
using static Samrt_Vehical_Hold.Extensions.FileExtensions;

namespace Samrt_Vehical_Hold.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : BaseController
    {
        private readonly IWebHostEnvironment _env;
        public VehiclesController(IWebHostEnvironment env,IDataService dataService)
            : base(dataService)
        {
            _env = env;
        }

        [HttpPost("GetVehiclesByNationalNumber")]
        [HttpPost]
        public async Task<IActionResult> GetVehiclesByNationalNumber([FromBody] VehicleRequestDto request)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var filePath = Path.Combine(_env.WebRootPath, "vehicles.json");
            var vehicleFileService = new VehicleFileService();
            var vehiclesFromFile = vehicleFileService.ReadVehiclesFromFile(filePath);

            var vehiclesForUser = vehiclesFromFile
                .Where(v => v.OwnerNationalNumber == request.NationalNumber)
                .ToList();

            if (request.IsInfo)
            {
                if (!vehiclesForUser.Any())
                {
                    return NotFound("YouDoNotOwnRegisteredCars");
                }

                var existingVehiclesInDb = await _dataService.GetQuery<Vehicle>()
                    .Where(v => v.OwnerNationalNumber == request.NationalNumber)
                    .ToListAsync();

                if (existingVehiclesInDb.Any())
                {
                    return Ok(new
                    {
                        Message = "YouAlreadyRegisterYourCar",
                        Data = existingVehiclesInDb
                    });
                }

                return Ok(new
                {
                    Message = "VehiclesFoundInFile",
                    Data = vehiclesForUser
                });
            }
            else
            {

                if (!vehiclesForUser.Any())
                    return NotFound("YouDoNotOwnRegisteredCars");

                var existingVehiclesInDb = await _dataService.GetQuery<Vehicle>()
                    .Where(v => v.OwnerNationalNumber == request.NationalNumber)
                    .ToListAsync();

                if (existingVehiclesInDb.Any())
                {
                    return Ok(new
                    {
                        Message = "YouAlreadyRegisterYourCar",
                        Data = existingVehiclesInDb
                    });
                }

                foreach (var vehicle in vehiclesForUser)
                {
                    vehicle.Id = Guid.NewGuid();
                    vehicle.OwnerUserId = userId.Value;
                    vehicle.RegistrationDate = DateTime.UtcNow;

                    await _dataService.AddAsync(vehicle);
                    await _dataService.SaveAsync();

                }

                return Ok(new
                {
                    Message = "VehiclesRegisteredSuccessfully",
                    Data = vehiclesForUser
                });
            }
        }
        [HttpPost]
        [Route("api/checkVehicleHold")]
        public IActionResult CheckVehicleHold([FromBody] VehicleHoldRequestDto request)
        {
            var filePath = Path.Combine(_env.WebRootPath, "VehicalHold.json");
            var holdFileService = new VehicleHoldFileService();
            var holds = holdFileService.ReadVehicleHoldsFromFile(filePath);

            var vehicleHold = holds.FirstOrDefault(h => h.PlateNumber.Equals(request.PlateNumber, StringComparison.OrdinalIgnoreCase));

            if (vehicleHold == null)
            {
                return Ok(new
                {
                    Message = "NoHoldOnVehicle"
                });
            }

            var holdEndDate = vehicleHold.RequestDate.AddDays(vehicleHold.HoldDurationDays);
            var timeRemaining = holdEndDate - DateTime.UtcNow;

            return Ok(new
            {
                Message = "VehicleHoldFound",
                Data = vehicleHold,
                TimeRemaining = timeRemaining.TotalHours > 0 ? timeRemaining.ToString(@"dd\.hh\:mm\:ss") : "Expired"
            });
        }


        [HttpPost]
        [Route("api/submitHoldRequest")]
        public async Task<IActionResult> SubmitHoldRequest([FromBody] HoldRequestDto request)
        {
            var filePath = Path.Combine(_env.WebRootPath, "VehicalHold.json");
            var holdFileService = new VehicleHoldFileService();
            var holds = holdFileService.ReadVehicleHoldsFromFile(filePath);

            var vehicleHold = holds.FirstOrDefault(h => h.PlateNumber.Equals(request.PlateNumber, StringComparison.OrdinalIgnoreCase));
            if (vehicleHold == null)
                return BadRequest("NoHoldOnVehicle");
            var userId = GetUserId();
            var vehicle = _dataService.GetQuery<Vehicle>().FirstOrDefault(v => v.Id == request.VehicleId && v.OwnerUserId == userId);
            if (vehicle == null)
                return BadRequest("VehicleNotBelongToUser");
            var endDate = request.StartDate.AddDays(vehicleHold.HoldDurationDays);

            var holdApplication = new HoldRequest
            {
                Id = Guid.NewGuid(),
                PlateNumber = request.PlateNumber,
                StartDate = request.StartDate,
                EndDate = endDate,
                Location = request.Location,
                RequestDate = DateTime.UtcNow,
                VehicleId = request.VehicleId,
                IsStart = false
            };

            await _dataService.AddAsync(holdApplication);
            await _dataService.SaveAsync();

            return Ok(new
            {
                Message = "HoldRequestSubmitted",
                Data = holdApplication
            });
        }

      

    }
}
