using System.Net.Http.Json;
using System.Text.Json;
using BourgPalette.DTOs;

namespace BourgPalette.Services;

public interface IWeatherService
{
    Task<List<WeatherSummaryDto>> GetCurrentAsync(IEnumerable<(double lat, double lon)> coords, CancellationToken ct);
    TypeMultipliersDto ComputeMultipliers(WeatherSummaryDto w);
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _http;
    public WeatherService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.open-meteo.com/");
    }

    public async Task<List<WeatherSummaryDto>> GetCurrentAsync(IEnumerable<(double lat, double lon)> coords, CancellationToken ct)
    {
        // Build comma-separated coordinates lists
        var latitudes = string.Join(',', coords.Select(c => c.lat.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var longitudes = string.Join(',', coords.Select(c => c.lon.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        var url = $"v1/forecast?latitude={latitudes}&longitude={longitudes}&models=meteofrance_seamless&current=temperature_2m,precipitation,weather_code";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var json = await res.Content.ReadAsStringAsync(ct);

        List<OpenMeteoItem> items = new();
        try
        {
            // Try single object first
            var one = JsonSerializer.Deserialize<OpenMeteoItem>(json, options);
            if (one != null && (one.Latitude != null || one.Current != null))
            {
                items.Add(one);
            }
            else
            {
                // Try array of objects
                var many = JsonSerializer.Deserialize<List<OpenMeteoItem>>(json, options);
                if (many != null)
                    items = many;
            }
        }
        catch
        {
            // Last resort: look for a "results" array wrapper
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
                {
                    items = new List<OpenMeteoItem>();
                    foreach (var el in results.EnumerateArray())
                    {
                        var obj = el.Deserialize<OpenMeteoItem>(options);
                        if (obj != null) items.Add(obj);
                    }
                }
            }
            catch
            {
                // ignore; items will remain empty and mapped below
            }
        }

        return items.Select(i => new WeatherSummaryDto
        {
            Latitude = i.Latitude,
            Longitude = i.Longitude,
            Time = i.Current?.Time,
            TemperatureC = i.Current?.Temperature2m,
            PrecipitationMm = i.Current?.Precipitation,
            WeatherCode = i.Current?.WeatherCode
        }).ToList();
    }

    public TypeMultipliersDto ComputeMultipliers(WeatherSummaryDto w)
    {
        // Simple example rules (you can refine):
        // - Rain (weather_code ~ 51-67,80-82): +20% Water, -10% Fire
        // - Clear/Sunny (weather_code 0): +15% Fire, -10% Water
        // - Overcast/Clouds (1-3): +5% Electric
        // - Snow (71-77,85-86): +20% Ice
        // - Thunderstorm (95-99): +25% Electric
        // - High precipitation (>5mm): +10% Water additionally

        var m = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var code = w.WeatherCode ?? 0;
        var precip = w.PrecipitationMm ?? 0;

        void Boost(string type, double pct)
        {
            if (!m.TryGetValue(type, out var cur)) cur = 0;
            m[type] = cur + pct; // additive bonuses in percentage points
        }

        bool inRange(int x, int a, int b) => x >= a && x <= b;

        if (code == 0)
        {
            Boost("Fire", 0.15);
            Boost("Water", -0.10);
        }
        else if (inRange(code, 1, 3))
        {
            Boost("Electric", 0.05);
        }
        else if (inRange(code, 51, 67) || inRange(code, 80, 82))
        {
            Boost("Water", 0.20);
            Boost("Fire", -0.10);
        }
        else if (inRange(code, 71, 77) || inRange(code, 85, 86))
        {
            Boost("Ice", 0.20);
        }
        else if (inRange(code, 95, 99))
        {
            Boost("Electric", 0.25);
        }

        if (precip > 5)
        {
            Boost("Water", 0.10);
        }

        return new TypeMultipliersDto { Multipliers = m };
    }
}