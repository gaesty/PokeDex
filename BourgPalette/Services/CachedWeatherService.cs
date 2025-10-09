using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using BourgPalette.DTOs;

namespace BourgPalette.Services;

public class CachedWeatherService : IWeatherService
{
    private readonly WeatherService _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedWeatherService> _logger;

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
                if (fromCache != null) return fromCache;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis get failed for key {Key}", key);
        }

        var fresh = await _inner.GetCurrentAsync(list, ct);

        try
        {
            var json = JsonSerializer.Serialize(fresh);
            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis set failed for key {Key}", key);
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
