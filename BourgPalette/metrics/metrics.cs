using Prometheus;

namespace BourgPalette.Metrics;

public static class AppMetrics
{
    public static readonly Counter NumberTotalPokemon = Prometheus.Metrics.CreateCounter(
        "number_total_pokemon", "Quantity of all pokemon in pokedex DB", new CounterConfiguration
        {});
    
    public static readonly Counter NumberTotalSpecies = Prometheus.Metrics.CreateCounter(
        "number_total_species", "Quantity of all species in pokedex DB", new CounterConfiguration
        {});

    // Domain-level operations counter (labels: entity, operation, outcome)
    // Examples:
    //  - entity: species|pokemon|move|type|team|media
    //  - operation: create|read|update|delete|list
    //  - outcome: success|error
    public static readonly Counter PokedexDomainOperationsTotal = Prometheus.Metrics.CreateCounter(
        "pokedex_domain_operations_total",
        "Total number of domain operations executed in the PokeDex API",
        new CounterConfiguration { LabelNames = new[] { "entity", "operation", "outcome" } });

    // Database query duration (seconds) by entity and operation
    public static readonly Histogram PokedexDbQueryDurationSeconds = Prometheus.Metrics.CreateHistogram(
        "pokedex_db_query_duration_seconds",
        "Duration of database queries in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "entity", "operation" },
            // 5ms → ~20s
            Buckets = Histogram.ExponentialBuckets(0.005, 2, 12)
        });

    // Current entities count by type (use a Gauge since totals may decrease if rows are deleted)
    public static readonly Gauge PokedexEntitiesGauge = Prometheus.Metrics.CreateGauge(
        "pokedex_entities_gauge",
        "Current number of entities in the database",
        new GaugeConfiguration { LabelNames = new[] { "entity" } });

    // Authentication events (signup/login/refresh/revoke) with outcome labels
    public static readonly Counter PokedexAuthEventsTotal = Prometheus.Metrics.CreateCounter(
        "pokedex_auth_events_total",
        "Authentication-related events in the API",
        new CounterConfiguration { LabelNames = new[] { "event", "outcome" } });
}
    