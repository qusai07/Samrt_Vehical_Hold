using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Samrt_Vehical_Hold.DTO.Vehicle;
using Samrt_Vehical_Hold.Entities;
using Samrt_Vehical_Hold.Helpers.Service;
using Samrt_Vehical_Hold.Repo.Interface;
using System.Drawing;

namespace Samrt_Vehical_Hold.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;

        private readonly IWebHostEnvironment _env;

        public VehiclesController(IWebHostEnvironment env,IDataService dataService, IConfiguration configuration, JwtHelper jwtHelper)
            : base(dataService)
        {
            _configuration = configuration;
            _jwtHelper = jwtHelper;
            _env = env;

        }

        [Authorize]
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
                // مرحلة الاستعلام فقط
                if (!vehiclesForUser.Any())
                {
                    return NotFound("YouDoNotOwnRegisteredCars");
                }

                // شيك إذا مسجلها بالداتا بيز
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

                // رجع سيارات الملف (لأنه عنده سيارات لكن مش مسجلة بعد)
                return Ok(new
                {
                    Message = "VehiclesFoundInFile",
                    Data = vehiclesForUser
                });
            }
            else
            {
                // مرحلة التسجيل

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

\                foreach (var vehicle in vehiclesForUser)
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
                    Message = "NoHoldOnVehicle",
                    Data = (object)null
                });
            }

            return Ok(new
            {
                Message = "VehicleHoldFound",
                Data = vehicleHold
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

            var endDate = request.StartDate.AddDays(vehicleHold.HoldDurationDays);

            var holdApplication = new HoldRequest
            {
                Id = Guid.NewGuid(),
                PlateNumber = request.PlateNumber,
                StartDate = request.StartDate,
                EndDate = endDate,
                Location = request.Location,
                RequestDate = DateTime.UtcNow
            };

            await _dataService.AddAsync(holdApplication);
            await _dataService.SaveAsync();

            return Ok(new
            {
                Message = "HoldRequestSubmitted",
                Data = holdApplication
            });
        }


        public class VehicleFileService
        {
            public List<Vehicle> ReadVehiclesFromFile(string filePath)
            {
                var json = System.IO.File.ReadAllText(filePath);

                return JsonConvert.DeserializeObject<List<Vehicle>>(json);
            }


        }
        public class VehicleHoldFileService
        {
            public List<VehicleHold> ReadVehicleHoldsFromFile(string filePath)
            {
                var json = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<VehicleHold>>(json);
            }
        }

    }
}
