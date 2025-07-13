using Microsoft.EntityFrameworkCore;
using Samrt_Vehical_Hold.Data;
using Samrt_Vehical_Hold.Infastructure;

namespace Samrt_Vehical_Hold.Startup
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddBusinessServiceModule();
        }
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        public static void AddCorsPolicies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalNetwork", policy =>
                {
                    policy.WithOrigins("http://192.168.1.115:5300")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }
    }
}
