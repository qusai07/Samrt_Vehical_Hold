using Microsoft.AspNetCore.Mvc;
using Samrt_Vehical_Hold.Models;
using Samrt_Vehical_Hold.Repo.Interface;
using System.Security.Claims;

namespace Samrt_Vehical_Hold.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly IDataService _dataService;

        public BaseController(IDataService dataService)
        {
            _dataService = dataService;
        }

        protected async Task<ApplicationUser> GetUserByIdAsync(Guid userId)
        {
            return await _dataService.GetByIdAsync<ApplicationUser>(userId);
        }
        //helper method
        protected Guid? GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out var guid))
                return guid;
            return null;
        }

    }

}
