using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samrt_Vehical_Hold.DTO.Violation;
using Samrt_Vehical_Hold.Entities;
using Samrt_Vehical_Hold.Helpers.Service;
using Samrt_Vehical_Hold.Models;
using Samrt_Vehical_Hold.Repo.Interface;
namespace Samrt_Vehical_Hold.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViolationsController : BaseController
    {
        public ViolationsController(IDataService dataService) : base(dataService){}

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("api/GetAllViolations")]
        public IActionResult GetAllViolations()
        {
            var violations = _dataService.GetQuery<Violation>()
                             .Include(v => v.HoldRequest)
                             .ToList();

            if (!violations.Any())
                return NotFound("NoViolationsFound");

            return Ok(violations);
        }
        [HttpGet]
        [Route("api/violations/active-moved-vehicles")]
        public IActionResult GetActiveMovedVehicles()
        {
            var violations = _dataService.GetQuery<Violation>()
                             .Include(v => v.HoldRequest)
                             .Where(v => v.HoldRequest.IsStart)
                             .ToList();

            if (!violations.Any())
                return NotFound("NoActiveMovedVehicles");

            return Ok(violations);
        }

        [HttpPost]
        [Route("api/violations/change-location")]
        public async Task<IActionResult> ChangeCarLocation([FromBody] ChangeCarLocationDto dto)
        {
            var holdRequest = _dataService.GetQuery<HoldRequest>()
                .FirstOrDefault(h => h.Id == dto.HoldRequestId);

            if (holdRequest == null)
                return NotFound("HoldRequestNotFound");

            Violation violation = null;
            if (holdRequest.IsStart)
            {
                violation = new Violation
                {
                    ID = Guid.NewGuid(),
                    HoldRequestId = dto.HoldRequestId,
                    Description = $"Vehicle moved to new location: {dto.NewLocation}",
                    ViolationDate = DateTime.UtcNow,
                    IsResolved = false
                };

                await _dataService.AddAsync(violation);
            }

            holdRequest.Location = dto.NewLocation;
            await _dataService.UpdateAsync(holdRequest);
            await _dataService.SaveAsync();

            return Ok(new
            {
                Message = "CarLocationUpdatedSuccessfully",
                NewLocation = holdRequest.Location,
                ViolationAdded = violation != null,
                Violation = violation
            });
        }
        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("api/GetViolationsForUser")]
        public IActionResult GetViolationsForUser()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var violations = _dataService.GetQuery<Violation>()
                .Include(v => v.HoldRequest)
                    .ThenInclude(h => h.Vehicle)
                .Where(v => v.HoldRequest.Vehicle.OwnerUserId == userId)
                .ToList();

            if (!violations.Any())
                return NotFound("NoViolationsFound");

            return Ok(violations);
        }



    }
}
