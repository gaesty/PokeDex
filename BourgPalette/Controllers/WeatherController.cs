using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BourgPalette.Services;
using BourgPalette.Constants;

namespace BourgPalette.Controllers;

[ApiController]
[Route("api/weather")]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weather;
    public WeatherController(IWeatherService weather)
    {
        _weather = weather;
    }

    // GET api/weather/bonuses?lat=48.53&lon=2.3499
    [HttpGet("bonuses")]
    public async Task<IActionResult> GetBonuses([FromQuery] double lat, [FromQuery] double lon, CancellationToken ct)
    {
        var data = await _weather.GetCurrentAsync(new[] { (lat, lon) }, ct);
        if (data.Count == 0) return NotFound();
        var multipliers = _weather.ComputeMultipliers(data[0]);
        // Flatten to return the raw multipliers map directly
        return Ok(new { coords = new { lat, lon }, summary = data[0], multipliers = multipliers.Multipliers });
    }

    // GET api/weather/bonuses/by-city?name=Paris
    [HttpGet("bonuses/by-city")]
    public async Task<IActionResult> GetBonusesByCity([FromQuery] string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Le paramètre 'name' est requis.");
        if (!CityCoordinates.Cities.TryGetValue(name, out var coord))
            return NotFound($"Ville inconnue: {name}. Utilisez /api/weather/cities pour la liste.");

        var data = await _weather.GetCurrentAsync(new[] { (coord.lat, coord.lon) }, ct);
        if (data.Count == 0) return NotFound();
        var multipliers = _weather.ComputeMultipliers(data[0]);
        return Ok(new { city = name, coords = new { lat = coord.lat, lon = coord.lon }, summary = data[0], multipliers = multipliers.Multipliers });
    }

    // GET api/weather/summaries?lat=48.53,43.297&lon=2.3488,5.3811
    [HttpGet("summaries")]
    public async Task<IActionResult> GetSummaries([FromQuery] string lat, [FromQuery] string lon, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(lat) || string.IsNullOrWhiteSpace(lon))
            return BadRequest("lat et lon sont requis (listes séparées par des virgules).");

        var latParts = lat.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var lonParts = lon.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (latParts.Length != lonParts.Length)
            return BadRequest("Le nombre de latitudes doit correspondre au nombre de longitudes.");

        var pairs = new List<(double, double)>(latParts.Length);
        for (int i = 0; i < latParts.Length; i++)
        {
            if (!double.TryParse(latParts[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var la))
                return BadRequest($"Latitude invalide à l'index {i}: '{latParts[i]}'");
            if (!double.TryParse(lonParts[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lo))
                return BadRequest($"Longitude invalide à l'index {i}: '{lonParts[i]}'");
            pairs.Add((la, lo));
        }

        if (pairs.Count == 0)
            return BadRequest("Aucune paire de coordonnées valide.");

    var data = await _weather.GetCurrentAsync(pairs, ct);
    var coordsEcho = pairs.Select(p => new { lat = p.Item1, lon = p.Item2 }).ToList();
    return Ok(new { coords = coordsEcho, summaries = data });
    }

    // GET api/weather/summaries/by-cities?names=Paris,Marseille,Brest
    [HttpGet("summaries/by-cities")]
    public async Task<IActionResult> GetSummariesByCities([FromQuery] string names, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(names))
            return BadRequest("Le paramètre 'names' est requis. Exemple: Paris,Marseille,Brest");

        var parts = names.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var coords = new List<(double, double)>();
        var unknown = new List<string>();
        foreach (var p in parts)
        {
            if (CityCoordinates.Cities.TryGetValue(p, out var c))
                coords.Add((c.lat, c.lon));
            else
                unknown.Add(p);
        }
        if (coords.Count == 0)
            return BadRequest("Aucune ville reconnue. Utilisez /api/weather/cities pour la liste.");

        var data = await _weather.GetCurrentAsync(coords, ct);
        var coordList = parts
            .Select(p => new {
                name = p,
                hasCoords = CityCoordinates.Cities.TryGetValue(p, out var cc),
                lat = CityCoordinates.Cities.TryGetValue(p, out var c2) ? c2.lat : (double?)null,
                lon = CityCoordinates.Cities.TryGetValue(p, out var c3) ? c3.lon : (double?)null
            })
            .ToList();
        return Ok(new { cities = parts, coords = coordList, summaries = data, unknown });
    }

    // GET api/weather/cities -> list available city names and coordinates
    [HttpGet("cities")]
    public IActionResult GetAvailableCities()
    {
        var list = CityCoordinates.Cities
            .Select(kv => new { name = kv.Key, lat = kv.Value.lat, lon = kv.Value.lon })
            .OrderBy(x => x.name)
            .ToList();
        return Ok(list);
    }
}