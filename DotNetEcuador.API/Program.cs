using DotNetEcuador.API.Configuration;
using Serilog;
using DotNetEcuador.API.Services;
using DotNetEcuador.API.Middleware;
using DotNetEcuador.API.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();

builder.Services.ConfigureMongoDB(builder.Configuration);
builder.Services.ConfigureApiVersioning();
builder.Services.ConfigureAuthentication(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureSwagger();
builder.Services.ConfigureOptions<DotNetEcuador.API.Configuration.ConfigureSwaggerOptions>();

builder.Services.ConfigureApplicationServices(builder.Configuration);
builder.Services.AddSingleton<IMessageService, MessageService>();
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

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
    app.UseSwaggerWithVersioning(apiVersionDescriptionProvider);
}

app.UseCors();
app.UseHttpsRedirection();
app.UseHealthCheckEndpoints();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
