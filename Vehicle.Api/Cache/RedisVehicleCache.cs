using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Vehicle.Api.Entities;

namespace Vehicle.Api.Cache;

/// <summary>
/// Класс для работы с Redis-кэшем транспортных средств.
/// </summary>
public class RedisVehicleCache(IDistributedCache cache) : IVehicleCache
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IDistributedCache _cache = cache;

    /// <summary>
    /// Получает список объектов из кэша по ключу.
    /// </summary>
    /// <param name="key">Ключ кэша.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список объектов или null, если данных нет.</returns>
    public async Task<IReadOnlyList<VehicleEntity>?> GetAsync(string key,  CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<List<VehicleEntity>>(json, _jsonOptions);
    }

    /// <summary>
    /// Сохраняет список объектов в кэш.
    /// </summary>
    /// <param name="key">Ключ кэша.</param>
    /// <param name="vehicles">Список объектов.</param>
    /// <param name="ttl">Время хранения в кэше.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public async Task SetAsync(
        string key,
        IReadOnlyList<VehicleEntity> vehicles,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(vehicles, _jsonOptions);

        await _cache.SetStringAsync(
            key,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);
    }

    /// <summary>
    /// Получает один объект из кэша по ключу.
    /// </summary>
    /// <param name="key">Ключ кэша.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Объект или null, если данных нет.</returns>
    public async Task<VehicleEntity?> GetOneAsync(string key, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<VehicleEntity>(json, _jsonOptions);
    }

    /// <summary>
    /// Сохраняет один объект в кэш.
    /// </summary>
    /// <param name="key">Ключ кэша.</param>
    /// <param name="vehicle">Объект транспортного средства.</param>
    /// <param name="ttl">Время хранения в кэше.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public async Task SetOneAsync(
        string key,
        VehicleEntity vehicle,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(vehicle, _jsonOptions);

        await _cache.SetStringAsync(
            key,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);
    }
}
