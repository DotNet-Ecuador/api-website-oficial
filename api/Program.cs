using api.Services;
using DotNetEnv;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env
Env.Load();

// Retrieve values from .env
string mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
    ?? throw new Exception("MONGO_CONNECTION_STRING variable is missing in .env");
string mongoDatabaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE")
    ?? throw new Exception("MONGO_DATABASE variable is missing in .env");

// Configure MongoDB
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<CommunityService>();
builder.Services.AddScoped<VolunteerApplicationService>();
builder.Services.AddScoped<AreaOfInterestService>();

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
