using System.Collections.Generic;

namespace BourgPalette.Models;

public class Species
{
    public int Id { get; set; }
    public int? PokedexNumber { get; set; }
    public string? NameFr { get; set; }
    public string? NameEn { get; set; }
    public int? Generation { get; set; }
    public string? Region { get; set; }
    public ICollection<Pokemon> Pokemons { get; set; } = new List<Pokemon>();
}

public class Media
{
    public int Id { get; set; }
    public string? SpriteUrl { get; set; }
    public string? ArtworkUrl { get; set; }
    public string? Note { get; set; }
    public ICollection<Pokemon> Pokemons { get; set; } = new List<Pokemon>();
}

public class Pokemon
{
    public int Id { get; set; }
    public int? SpeciesId { get; set; }
    public Species? Species { get; set; }
    public string? Form { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public int? BaseExperience { get; set; }
    public int? ImageId { get; set; }
    public Media? Image { get; set; }

    public ICollection<PokemonType> PokemonTypes { get; set; } = new List<PokemonType>();
    public ICollection<PokemonAbility> PokemonAbilities { get; set; } = new List<PokemonAbility>();
    public ICollection<Stat> Stats { get; set; } = new List<Stat>();
    public ICollection<Evolution> FromEvolutions { get; set; } = new List<Evolution>();
    public ICollection<Evolution> ToEvolutions { get; set; } = new List<Evolution>();
    public ICollection<TeamPokemon> TeamPokemons { get; set; } = new List<TeamPokemon>();
}

public class TypeDefinition
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<PokemonType> PokemonTypes { get; set; } = new List<PokemonType>();
    public ICollection<Move> Moves { get; set; } = new List<Move>();
}

public class PokemonType
{
    public int PokemonId { get; set; }
    public Pokemon? Pokemon { get; set; }
    public int TypeId { get; set; }
    public TypeDefinition? TypeDefinition { get; set; }
}

public class Ability
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<PokemonAbility> PokemonAbilities { get; set; } = new List<PokemonAbility>();
}

public class PokemonAbility
{
    public int PokemonId { get; set; }
    public Pokemon? Pokemon { get; set; }
    public int AbilityId { get; set; }
    public Ability? Ability { get; set; }
    public bool? IsHidden { get; set; }
}

public class Move
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? TypeId { get; set; }
    public TypeDefinition? Type { get; set; }
    public int? Power { get; set; }
    public double? Accuracy { get; set; }
    public int? PP { get; set; }
    public string? Category { get; set; }
}

public class Stat
{
    public int Id { get; set; }
    public int? PokemonId { get; set; }
    public Pokemon? Pokemon { get; set; }
    public int? HP { get; set; }
    public int? Attack { get; set; }
    public int? Defense { get; set; }
    public int? SpecialAttack { get; set; }
    public int? SpecialDefense { get; set; }
    public int? Speed { get; set; }
}

public class Evolution
{
    public int Id { get; set; }
    public int? FromPokemonId { get; set; }
    public Pokemon? FromPokemon { get; set; }
    public int? ToPokemonId { get; set; }
    public Pokemon? ToPokemon { get; set; }
    public string? Condition { get; set; }
}

public class Team
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Owner { get; set; }
    public ICollection<TeamPokemon> TeamPokemons { get; set; } = new List<TeamPokemon>();
}

public class TeamPokemon
{
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int PokemonId { get; set; }
    public Pokemon? Pokemon { get; set; }
    public int? Slot { get; set; }
}
