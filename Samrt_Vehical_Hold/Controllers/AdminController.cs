using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Samrt_Vehical_Hold.DTO;
using Samrt_Vehical_Hold.DTO.VehicleHold;
using Samrt_Vehical_Hold.Entities;
using Samrt_Vehical_Hold.Helpers.Service;
using Samrt_Vehical_Hold.Models;
using Samrt_Vehical_Hold.Repo.Interface;

namespace Samrt_Vehical_Hold.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(IDataService dataService) : base(dataService){}
        [HttpPost]
        [Route("api/holdRequests/start")]
        public async Task<IActionResult> StartHoldRequest([FromBody] StartHoldRequestDto request)
        {
            var holdRequest =  _dataService.GetQuery<HoldRequest>()
                .FirstOrDefault(h => h.Id == request.RequestId);

            if (holdRequest == null)
                return NotFound("HoldRequestNotFound");

            holdRequest.IsStart = true;

            await _dataService.UpdateAsync(holdRequest);
            await _dataService.SaveAsync();

            return Ok(new
            {
                Message = "HoldRequestStarted",
                Data = holdRequest
            });
        }
        [HttpGet]
        [Route("api/GetAllHoldRequests")]
        public IActionResult GetAllHoldRequests()
        {
            var holdRequests = _dataService.GetQuery<HoldRequest>().ToList();
            if (!holdRequests.Any())
                return NotFound("HoldRequestsNotFound");
            return Ok(holdRequests);
        }


    }
}
