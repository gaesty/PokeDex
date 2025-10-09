using BourgPalette.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BourgPalette.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<BourgPalette.Models.Species> Species => Set<BourgPalette.Models.Species>();
    public DbSet<BourgPalette.Models.Media> Media => Set<BourgPalette.Models.Media>();
    public DbSet<BourgPalette.Models.Pokemon> Pokemons => Set<BourgPalette.Models.Pokemon>();
    public DbSet<BourgPalette.Models.TypeDefinition> Types => Set<BourgPalette.Models.TypeDefinition>();
    public DbSet<BourgPalette.Models.PokemonType> PokemonTypes => Set<BourgPalette.Models.PokemonType>();
    public DbSet<BourgPalette.Models.Ability> Abilities => Set<BourgPalette.Models.Ability>();
    public DbSet<BourgPalette.Models.PokemonAbility> PokemonAbilities => Set<BourgPalette.Models.PokemonAbility>();
    public DbSet<BourgPalette.Models.Move> Moves => Set<BourgPalette.Models.Move>();
    public DbSet<BourgPalette.Models.Stat> Stats => Set<BourgPalette.Models.Stat>();
    public DbSet<BourgPalette.Models.Evolution> Evolutions => Set<BourgPalette.Models.Evolution>();
    public DbSet<BourgPalette.Models.Team> Teams => Set<BourgPalette.Models.Team>();
    public DbSet<BourgPalette.Models.TeamPokemon> TeamPokemons => Set<BourgPalette.Models.TeamPokemon>();
    public DbSet<TokenInfo> TokenInfos => Set<TokenInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table names
    modelBuilder.Entity<BourgPalette.Models.Species>().ToTable("species");
    modelBuilder.Entity<BourgPalette.Models.Media>().ToTable("media");
    modelBuilder.Entity<BourgPalette.Models.Pokemon>().ToTable("pokemon");
    modelBuilder.Entity<BourgPalette.Models.TypeDefinition>().ToTable("types");
    modelBuilder.Entity<BourgPalette.Models.PokemonType>().ToTable("pokemon_types");
    modelBuilder.Entity<BourgPalette.Models.Ability>().ToTable("abilities");
    modelBuilder.Entity<BourgPalette.Models.PokemonAbility>().ToTable("pokemon_abilities");
    modelBuilder.Entity<BourgPalette.Models.Move>().ToTable("moves");
    modelBuilder.Entity<BourgPalette.Models.Stat>().ToTable("stats");
    modelBuilder.Entity<BourgPalette.Models.Evolution>().ToTable("evolutions");
    modelBuilder.Entity<BourgPalette.Models.Team>().ToTable("teams");
    modelBuilder.Entity<BourgPalette.Models.TeamPokemon>().ToTable("team_pokemons");

        // Keys and relationships
    modelBuilder.Entity<BourgPalette.Models.PokemonType>().HasKey(pt => new { pt.PokemonId, pt.TypeId });
    modelBuilder.Entity<BourgPalette.Models.PokemonAbility>().HasKey(pa => new { pa.PokemonId, pa.AbilityId });
    modelBuilder.Entity<BourgPalette.Models.TeamPokemon>().HasKey(tp => new { tp.TeamId, tp.PokemonId });

        modelBuilder.Entity<BourgPalette.Models.Pokemon>()
            .HasOne(p => p.Species)
            .WithMany(s => s.Pokemons)
            .HasForeignKey(p => p.SpeciesId);

        modelBuilder.Entity<BourgPalette.Models.Pokemon>()
            .HasOne(p => p.Image)
            .WithMany(m => m.Pokemons)
            .HasForeignKey(p => p.ImageId);

        modelBuilder.Entity<BourgPalette.Models.PokemonType>()
            .HasOne(pt => pt.Pokemon)
            .WithMany(p => p.PokemonTypes)
            .HasForeignKey(pt => pt.PokemonId);

        modelBuilder.Entity<BourgPalette.Models.PokemonType>()
            .HasOne(pt => pt.TypeDefinition)
            .WithMany(t => t.PokemonTypes)
            .HasForeignKey(pt => pt.TypeId);

        modelBuilder.Entity<BourgPalette.Models.PokemonAbility>()
            .HasOne(pa => pa.Pokemon)
            .WithMany(p => p.PokemonAbilities)
            .HasForeignKey(pa => pa.PokemonId);

        modelBuilder.Entity<BourgPalette.Models.PokemonAbility>()
            .HasOne(pa => pa.Ability)
            .WithMany(a => a.PokemonAbilities)
            .HasForeignKey(pa => pa.AbilityId);

        modelBuilder.Entity<BourgPalette.Models.Move>()
            .HasOne(m => m.Type)
            .WithMany(t => t.Moves)
            .HasForeignKey(m => m.TypeId);

        modelBuilder.Entity<BourgPalette.Models.Stat>()
            .HasOne(s => s.Pokemon)
            .WithMany(p => p.Stats)
            .HasForeignKey(s => s.PokemonId);

        modelBuilder.Entity<BourgPalette.Models.Evolution>()
            .HasOne(e => e.FromPokemon)
            .WithMany(p => p.FromEvolutions)
            .HasForeignKey(e => e.FromPokemonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BourgPalette.Models.Evolution>()
            .HasOne(e => e.ToPokemon)
            .WithMany(p => p.ToEvolutions)
            .HasForeignKey(e => e.ToPokemonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BourgPalette.Models.TeamPokemon>()
            .HasOne(tp => tp.Team)
            .WithMany(t => t.TeamPokemons)
            .HasForeignKey(tp => tp.TeamId);

        modelBuilder.Entity<BourgPalette.Models.TeamPokemon>()
            .HasOne(tp => tp.Pokemon)
            .WithMany(p => p.TeamPokemons)
            .HasForeignKey(tp => tp.PokemonId);

        // Column names alignment
        modelBuilder.Entity<BourgPalette.Models.Species>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.PokedexNumber).HasColumnName("pokedex_number");
            e.Property(p => p.NameFr).HasColumnName("name_fr");
            e.Property(p => p.NameEn).HasColumnName("name_en");
        });

        modelBuilder.Entity<BourgPalette.Models.Pokemon>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.SpeciesId).HasColumnName("species_id");
            e.Property(p => p.Form).HasColumnName("form");
            e.Property(p => p.Height).HasColumnName("height");
            e.Property(p => p.Weight).HasColumnName("weight");
            e.Property(p => p.BaseExperience).HasColumnName("base_experience");
            e.Property(p => p.ImageId).HasColumnName("image_id");
        });

        modelBuilder.Entity<BourgPalette.Models.Media>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.SpriteUrl).HasColumnName("sprite_url");
            e.Property(p => p.ArtworkUrl).HasColumnName("artwork_url");
            e.Property(p => p.Note).HasColumnName("note");
        });

        modelBuilder.Entity<BourgPalette.Models.TypeDefinition>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.Description).HasColumnName("description");
        });

        modelBuilder.Entity<BourgPalette.Models.Ability>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.Description).HasColumnName("description");
        });

        modelBuilder.Entity<BourgPalette.Models.PokemonType>(e =>
        {
            e.Property(p => p.PokemonId).HasColumnName("pokemon_id");
            e.Property(p => p.TypeId).HasColumnName("type_id");
        });

        modelBuilder.Entity<BourgPalette.Models.PokemonAbility>(e =>
        {
            e.Property(p => p.PokemonId).HasColumnName("pokemon_id");
            e.Property(p => p.AbilityId).HasColumnName("ability_id");
            e.Property(p => p.IsHidden).HasColumnName("is_hidden");
        });

        modelBuilder.Entity<BourgPalette.Models.Move>(e =>
        {
            e.Property(m => m.Id).HasColumnName("id");
            e.Property(m => m.TypeId).HasColumnName("type_id");
            e.Property(m => m.Power).HasColumnName("power");
            e.Property(m => m.Accuracy).HasColumnName("accuracy");
            e.Property(m => m.PP).HasColumnName("pp");
            e.Property(m => m.Category).HasColumnName("category");
        });

        modelBuilder.Entity<BourgPalette.Models.Stat>(e =>
        {
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.PokemonId).HasColumnName("pokemon_id");
            e.Property(s => s.HP).HasColumnName("hp");
            e.Property(s => s.Attack).HasColumnName("attack");
            e.Property(s => s.Defense).HasColumnName("defense");
            e.Property(s => s.SpecialAttack).HasColumnName("special_attack");
            e.Property(s => s.SpecialDefense).HasColumnName("special_defense");
            e.Property(s => s.Speed).HasColumnName("speed");
        });

        modelBuilder.Entity<BourgPalette.Models.Evolution>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.FromPokemonId).HasColumnName("from_pokemon_id");
            e.Property(p => p.ToPokemonId).HasColumnName("to_pokemon_id");
            e.Property(p => p.Condition).HasColumnName("condition");
        });

        modelBuilder.Entity<BourgPalette.Models.TeamPokemon>(e =>
        {
            e.Property(p => p.TeamId).HasColumnName("team_id");
            e.Property(p => p.PokemonId).HasColumnName("pokemon_id");
            e.Property(p => p.Slot).HasColumnName("slot");
        });

        modelBuilder.Entity<BourgPalette.Models.Team>(e =>
        {
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.Name).HasColumnName("name");
            e.Property(t => t.Description).HasColumnName("description");
            e.Property(t => t.Owner).HasColumnName("owner");
        });
    }
}
