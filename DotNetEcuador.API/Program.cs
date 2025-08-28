using DotNetEcuador.API.Configuration;
using DotNetEcuador.API.Infraestructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB
builder.Services.ConfigureMongoDB();

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddScoped<CommunityService>();
builder.Services.AddScoped<IVolunteerApplicationService, VolunteerApplicationService>();
builder.Services.AddScoped<IAreaOfInterestService, AreaOfInterestService>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
