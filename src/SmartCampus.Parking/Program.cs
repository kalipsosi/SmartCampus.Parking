using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SmartCampus.Parking.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(_ =>
    NpgsqlDataSource.Create(builder.Configuration.GetConnectionString("ParkingDb")!));
builder.Services.AddScoped<ParkingSessionRepository>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("parking-service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

var app = builder.Build();
app.MapControllers();
app.Run();
