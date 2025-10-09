using System.Text.Json.Serialization;

namespace BourgPalette.DTOs;

public class OpenMeteoCurrentUnits
{
    [JsonPropertyName("time")] public string? Time { get; set; }
    [JsonPropertyName("interval")] public string? Interval { get; set; }
    [JsonPropertyName("temperature_2m")] public string? Temperature2m { get; set; }
    [JsonPropertyName("precipitation")] public string? Precipitation { get; set; }
    [JsonPropertyName("weather_code")] public string? WeatherCode { get; set; }
}

public class OpenMeteoCurrent
{
    [JsonPropertyName("time")] public string? Time { get; set; }
    [JsonPropertyName("interval")] public int? Interval { get; set; }
    [JsonPropertyName("temperature_2m")] public double? Temperature2m { get; set; }
    [JsonPropertyName("precipitation")] public double? Precipitation { get; set; }
    [JsonPropertyName("weather_code")] public int? WeatherCode { get; set; }
}

public class OpenMeteoItem
{
    [JsonPropertyName("latitude")] public double? Latitude { get; set; }
    [JsonPropertyName("longitude")] public double? Longitude { get; set; }
    [JsonPropertyName("timezone")] public string? Timezone { get; set; }
    [JsonPropertyName("elevation")] public double? Elevation { get; set; }
    [JsonPropertyName("current_units")] public OpenMeteoCurrentUnits? CurrentUnits { get; set; }
    [JsonPropertyName("current")] public OpenMeteoCurrent? Current { get; set; }
}

public class WeatherSummaryDto
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Time { get; set; }
    public double? TemperatureC { get; set; }
    public double? PrecipitationMm { get; set; }
    public int? WeatherCode { get; set; }
}

public class TypeMultipliersDto
{
    public Dictionary<string, double> Multipliers { get; set; } = new();
}