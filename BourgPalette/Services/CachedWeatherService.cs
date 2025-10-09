using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using BourgPalette.DTOs;
using Prometheus;

namespace BourgPalette.Services;

public class CachedWeatherService : IWeatherService
{
    private readonly WeatherService _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedWeatherService> _logger;

    private static readonly Counter CacheGetsTotal = Metrics.CreateCounter(
        "weather_cache_gets_total",
        "Total number of cache get attempts for weather data",
        new CounterConfiguration { LabelNames = new[] { "result" } });

    private static readonly Counter CacheSetsTotal = Metrics.CreateCounter(
        "weather_cache_sets_total",
        "Total number of cache set operations for weather data",
        new CounterConfiguration { LabelNames = new[] { "result" } });

    private static readonly Gauge CacheLastTtlSeconds = Metrics.CreateGauge(
        "weather_cache_last_ttl_seconds",
        "TTL in seconds used for the last cached weather entry");

    public CachedWeatherService(WeatherService inner, IDistributedCache cache, ILogger<CachedWeatherService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<WeatherSummaryDto>> GetCurrentAsync(IEnumerable<(double lat, double lon)> coords, CancellationToken ct)
    {
        var list = coords.ToList();
        var key = BuildKey(list);
        try
        {
            var cached = await _cache.GetStringAsync(key, ct);
            if (!string.IsNullOrEmpty(cached))
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fromCache = JsonSerializer.Deserialize<List<WeatherSummaryDto>>(cached, options);
                if (fromCache != null)
                {
                    CacheGetsTotal.WithLabels("hit").Inc();
                    return fromCache;
                }
            }
            CacheGetsTotal.WithLabels("miss").Inc();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis get failed for key {Key}", key);
            CacheGetsTotal.WithLabels("error").Inc();
        }

        var fresh = await _inner.GetCurrentAsync(list, ct);

        try
        {
            var json = JsonSerializer.Serialize(fresh);
            var ttl = TimeSpan.FromMinutes(5);
            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
            CacheSetsTotal.WithLabels("ok").Inc();
            CacheLastTtlSeconds.Set(ttl.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis set failed for key {Key}", key);
            CacheSetsTotal.WithLabels("error").Inc();
        }

        return fresh;
    }

    public TypeMultipliersDto ComputeMultipliers(WeatherSummaryDto w)
        => _inner.ComputeMultipliers(w);

    private static string BuildKey(List<(double lat, double lon)> coords)
    {
        var sb = new StringBuilder("weather:");
        for (int i = 0; i < coords.Count; i++)
        {
            if (i > 0) sb.Append('|');
            sb.Append(coords[i].lat.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(coords[i].lon.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }
}
