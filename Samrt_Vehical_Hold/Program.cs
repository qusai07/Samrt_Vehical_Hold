using Samrt_Vehical_Hold.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsPolicies();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseGlobalMiddlewares();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
