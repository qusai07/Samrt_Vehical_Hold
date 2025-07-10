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
        public async Task<IActionResult> GetVehiclesByNationalNumber([FromBody] VehicleRequestDto request)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            List<Vehicle> matchedVehicles = new();

            if (request.IsInfo)
            {
                // من الملف
                var filePath = Path.Combine(_env.WebRootPath, "vehicles.json");
                var vehicleFileService = new VehicleFileService();
                var vehiclesFromFile = vehicleFileService.ReadVehiclesFromFile(filePath);

                matchedVehicles = vehiclesFromFile
                    .Where(v => v.OwnerNationalNumber == request.NationalNumber)
                    .ToList();

                if (!matchedVehicles.Any())
                    return NotFound("لا تملك سيارات مسجلة");

                return Ok(matchedVehicles);
            }
            else
            {
                matchedVehicles = await _dataService.GetQuery<Vehicle>()
                    .Where(v => v.OwnerNationalNumber == request.NationalNumber)
                    .ToListAsync();

                if (matchedVehicles.Any())
                {
                    return Ok(matchedVehicles);
                }
                else
                {
                    var filePath = Path.Combine(_env.WebRootPath, "vehicles.json");
                    var vehicleFileService = new VehicleFileService();
                    var vehiclesFromFile = vehicleFileService.ReadVehiclesFromFile(filePath);

                    var vehiclesToAdd = vehiclesFromFile
                        .Where(v => v.OwnerNationalNumber == request.NationalNumber)
                        .ToList();

                    if (!vehiclesToAdd.Any())
                        return NotFound("لا تملك سيارات مسجلة");

                    foreach (var vehicle in vehiclesToAdd)
                    {
                        vehicle.Id = Guid.NewGuid();
                        vehicle.OwnerUserId = userId.Value;
                        vehicle.RegistrationDate = DateTime.UtcNow;

                        await _dataService.AddAsync(vehicle);
                    }

                    return Ok(vehiclesToAdd);
                }
            }
        }

        public class VehicleFileService
        {
            public List<Vehicle> ReadVehiclesFromFile(string filePath)
            {
                var json = System.IO.File.ReadAllText(filePath);

                return JsonConvert.DeserializeObject<List<Vehicle>>(json);
            }


        }
    }
}
