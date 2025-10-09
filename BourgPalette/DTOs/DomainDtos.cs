using System.ComponentModel.DataAnnotations;

namespace BourgPalette.DTOs;

// Species
public record SpeciesListItemDto(
    int Id,
    int? PokedexNumber,
    string? NameFr,
    string? NameEn,
    int? Generation,
    string? Region
);

public class SpeciesCreateDto
{
    public int? PokedexNumber { get; set; }
    [MaxLength(100)] public string? NameFr { get; set; }
    [MaxLength(100)] public string? NameEn { get; set; }
    public int? Generation { get; set; }
    [MaxLength(50)] public string? Region { get; set; }
}

public class SpeciesUpdateDto : SpeciesCreateDto { }

// TypeDefinition (Types)
public record TypeListItemDto(int Id, string? Name, string? Description);
public class TypeCreateDto { [MaxLength(50)] public string? Name { get; set; } public string? Description { get; set; } }
public class TypeUpdateDto : TypeCreateDto { }

// Ability
public record AbilityListItemDto(int Id, string? Name, string? Description);
public class AbilityCreateDto { [MaxLength(50)] public string? Name { get; set; } public string? Description { get; set; } }
public class AbilityUpdateDto : AbilityCreateDto { }

// Move
public record MoveListItemDto(int Id, string? Name, string? Description, int? TypeId, int? Power, double? Accuracy, int? PP, string? Category);
public class MoveCreateDto
{
    [MaxLength(100)] public string? Name { get; set; }
    public string? Description { get; set; }
    public int? TypeId { get; set; }
    public int? Power { get; set; }
    public double? Accuracy { get; set; }
    public int? PP { get; set; }
    [MaxLength(50)] public string? Category { get; set; }
}
public class MoveUpdateDto : MoveCreateDto { }

// Team
public record TeamListItemDto(int Id, string? Name, string? Description, string? Owner);
public class TeamCreateDto { [MaxLength(100)] public string? Name { get; set; } public string? Description { get; set; } public string? Owner { get; set; } }
public class TeamUpdateDto : TeamCreateDto { }

public class TeamPokemonAddDto
{
    [Required] public int PokemonId { get; set; }
    public int? Slot { get; set; }
}

// Media
public record MediaListItemDto(int Id, string? SpriteUrl, string? ArtworkUrl, string? Note);
public class MediaCreateDto
{
    [Url] public string? SpriteUrl { get; set; }
    [Url] public string? ArtworkUrl { get; set; }
    [MaxLength(200)] public string? Note { get; set; }
}
public class MediaUpdateDto : MediaCreateDto { }
