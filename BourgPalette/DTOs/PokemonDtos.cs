using System.ComponentModel.DataAnnotations;

namespace BourgPalette.DTOs;

// Read model for list/detail
public record PokemonListItemDto(
    int Id,
    int? SpeciesId,
    int? PokedexNumber,
    string? SpeciesNameFr,
    string? SpeciesNameEn,
    string? Form,
    double? Height,
    double? Weight,
    int? BaseExperience,
    int? ImageId
);

// Create model
public record PokemonCreateDto(
    [property: Required]
    int? SpeciesId,

    [property: StringLength(50)]
    string? Form,

    [property: Range(0, 1000)]
    double? Height,

    [property: Range(0, 10000)]
    double? Weight,

    [property: Range(0, 10000)]
    int? BaseExperience,

    int? ImageId
);

// Update model (full replace semantics)
public record PokemonUpdateDto(
    [property: Required]
    int? SpeciesId,

    [property: StringLength(50)]
    string? Form,

    [property: Range(0, 1000)]
    double? Height,

    [property: Range(0, 10000)]
    double? Weight,

    [property: Range(0, 10000)]
    int? BaseExperience,

    int? ImageId
);
