using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Samrt_Vehical_Hold.Helpers.Service;
using Samrt_Vehical_Hold.Models;
using Samrt_Vehical_Hold.Repo.Impement;
using Samrt_Vehical_Hold.Repo.Interface;

namespace Samrt_Vehical_Hold.Infastructure
{
    public static class BusinessServiceModule
    {
        public static void AddBusinessServiceModule(this IServiceCollection Services)
        {
             Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
             Services.AddScoped<JwtHelper>();

             Services.AddScoped<IDataService, DataService>();

        }
    }
}
