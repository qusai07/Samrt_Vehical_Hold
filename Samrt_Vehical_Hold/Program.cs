using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Samrt_Vehical_Hold.Data;
using Samrt_Vehical_Hold.Helpers.Model;
using Samrt_Vehical_Hold.Models;
using System.Text;
using Samrt_Vehical_Hold.Helpers.Service;
using Samrt_Vehical_Hold.Repo.Impement;
using Samrt_Vehical_Hold.Repo.Interface;
using Samrt_Vehical_Hold.Infastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalNetwork", policy =>
    {
        policy.WithOrigins("http://192.168.1.115:5300") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});




// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // تعريف الـ Security Scheme للـ JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "أدخل الـ JWT Bearer token هنا. مثال: 'Bearer eyJhbGciOiJI...'"
    });

    // إضافة متطلب الأمان لجميع الـ APIs التي تحتاجه
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// Configure JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// read jwt settings instance for jwt authentication setup
var jwtSettingsSection = builder.Configuration.GetSection("Jwt");
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddBusinessServiceModule();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5500);
});




var app = builder.Build();
app.UseCors("AllowLocalNetwork");
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request from {context.Connection.RemoteIpAddress} to {context.Request.Path}");
    await next();
});

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();

}




app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
