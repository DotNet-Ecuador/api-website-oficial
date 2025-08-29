using DotNetEcuador.API.Configuration;
using DotNetEcuador.API.Services;
using DotNetEcuador.API.Middleware;
using DotNetEcuador.API.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB
builder.Services.ConfigureMongoDB();

// Configure API Versioning
builder.Services.ConfigureApiVersioning();

// Configure Authentication & Authorization
builder.Services.ConfigureAuthentication();

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

var app = builder.Build();

// Configure middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
    app.UseSwaggerWithVersioning(apiVersionDescriptionProvider);
}

app.UseHttpsRedirection();

// Configure Health Check endpoints
app.UseHealthCheckEndpoints();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
