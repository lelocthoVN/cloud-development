using Vehicle.Api.Cache;
using Vehicle.Api.Generation;
using Vehicle.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCors", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:7282",
                "https://localhost:7282",
                "http://localhost:5127",
                "https://localhost:5127")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<VehicleGenerator>();
builder.Services.AddScoped<IVehicleCache, RedisVehicleCache>();
builder.Services.AddScoped<VehicleService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
    options.InstanceName = "vehicle-api";
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ClientCors");

app.MapControllers();

app.Run();