using Microsoft.EntityFrameworkCore;

class PokedexDB : DbContext
{
    public PokedexDB(DbContextOptions<PokedexDB> options)
        : base(options) { }

    public DbSet<Pokemon> Pokemons => Set<Pokemon>();
}