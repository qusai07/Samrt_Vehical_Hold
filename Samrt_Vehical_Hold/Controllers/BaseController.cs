using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Samrt_Vehical_Hold.Models;
using Samrt_Vehical_Hold.Repo.Interface;

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
    }

}
