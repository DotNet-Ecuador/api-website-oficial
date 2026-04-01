using DotNetEcuador.API.Configuration;
using DotNetEcuador.API.Services;
using DotNetEcuador.API.Middleware;
using DotNetEcuador.API.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB
builder.Services.ConfigureMongoDB(builder.Configuration);

// Configure API Versioning
builder.Services.ConfigureApiVersioning();

// Configure Authentication & Authorization
builder.Services.ConfigureAuthentication(builder.Configuration);

// Add services to the container
builder.Services.AddControllers(options =>
{
    // Add global validation filter
    options.Filters.Add<ValidationActionFilter>();
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// Configure FluentValidation
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with versioning and JWT
builder.Services.ConfigureSwagger();
builder.Services.ConfigureOptions<DotNetEcuador.API.Configuration.ConfigureSwaggerOptions>();

// Configure Application Services
builder.Services.ConfigureApplicationServices();

// Configure Message Service (Singleton for minimal memory usage)
builder.Services.AddSingleton<IMessageService, MessageService>();

// Configure Health Checks
builder.Services.ConfigureHealthChecks();

builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddDefaultPolicy(policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
    }
    else
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',')
            ?? ["https://dotnetecuador.com"];
        options.AddDefaultPolicy(policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader());
    }
});

var app = builder.Build();

// Configure middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
    app.UseSwaggerWithVersioning(apiVersionDescriptionProvider);
}

app.UseCors();
app.UseHttpsRedirection();

// Configure Health Check endpoints
app.UseHealthCheckEndpoints();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
