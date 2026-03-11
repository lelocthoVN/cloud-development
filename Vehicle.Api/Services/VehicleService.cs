using System.Diagnostics;
using Vehicle.Api.Cache;
using Vehicle.Api.Entities;
using Vehicle.Api.Generation;

namespace Vehicle.Api.Services;

/// <summary>
/// Сервис для работы с транспортными средствами (генерация и получение по ID)
/// </summary>
public class VehicleService(
    VehicleGenerator generator,
    IVehicleCache vehicleCache,
    ILogger<VehicleService> logger)
{
    private readonly VehicleGenerator _generator = generator;
    private readonly IVehicleCache _vehicleCache = vehicleCache;
    private readonly ILogger<VehicleService> _logger = logger;

    /// <summary>
    /// Генерирует указанное количество транспортных средств и кэширует результат
    /// </summary>
    /// <param name="count">Количество транспортных средств для генерации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список сгенерированных транспортных средств</returns>
    public async Task<IReadOnlyList<VehicleEntity>> GenerateAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"vehicles:{count}";
        var stopwatch = Stopwatch.StartNew();

        var cachedVehicles = await _vehicleCache.GetAsync(cacheKey, cancellationToken);

        if (cachedVehicles is not null)
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "Cache hit for key {CacheKey}. Returned {Count} vehicles in {ElapsedMs} ms",
                cacheKey,
                cachedVehicles.Count,
                stopwatch.ElapsedMilliseconds);

            return cachedVehicles;
        }

        _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey);

        var vehicles = new List<VehicleEntity>(count);

        for (var i = 1; i <= count; i++)
        {
            vehicles.Add(_generator.Generate(i));
        }

        await _vehicleCache.SetAsync(
            cacheKey,
            vehicles,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        stopwatch.Stop();

        _logger.LogInformation(
            "Generated {Count} vehicles and cached them with key {CacheKey} in {ElapsedMs} ms",
            count,
            cacheKey,
            stopwatch.ElapsedMilliseconds);

        return vehicles;
    }

    /// <summary>
    /// Получает транспортное средство по ID (из кэша или генерирует новое)
    /// </summary>
    /// <param name="id">Идентификатор транспортного средства</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Транспортное средство с указанным ID</returns>
    public async Task<VehicleEntity> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"vehicle:{id}";
        var stopwatch = Stopwatch.StartNew();

        var cachedVehicle = await _vehicleCache.GetOneAsync(cacheKey, cancellationToken);

        if (cachedVehicle is not null)
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "Cache hit for key {CacheKey}. Returned vehicle with id {Id} in {ElapsedMs} ms",
                cacheKey,
                id,
                stopwatch.ElapsedMilliseconds);

            return cachedVehicle;
        }

        _logger.LogInformation(
            "Cache miss for key {CacheKey}. Generating vehicle with id {Id}",
            cacheKey,
            id);

        var vehicle = _generator.Generate(id);

        await _vehicleCache.SetOneAsync(
            cacheKey,
            vehicle,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        stopwatch.Stop();

        _logger.LogInformation(
            "Generated vehicle with id {Id} and cached it with key {CacheKey} in {ElapsedMs} ms",
            id,
            cacheKey,
            stopwatch.ElapsedMilliseconds);

        return vehicle;
    }
}