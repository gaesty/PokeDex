namespace BourgPalette.Constants;

public static class CityCoordinates
{
    // Preselected French cities and their coordinates as provided
    public static readonly IReadOnlyDictionary<string, (double lat, double lon)> Cities =
        new Dictionary<string, (double lat, double lon)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Paris"] = (48.534, 2.3488),
            ["Marseille"] = (43.297, 5.3811),
            ["Brest"] = (48.3903, -4.4863),
            ["Strasbourg"] = (48.5839, 7.7455),
            ["Lille"] = (50.633, 3.0586)
        };
}
