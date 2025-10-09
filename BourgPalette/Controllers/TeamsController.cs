using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BourgPalette.Data;
using BourgPalette.Models;
using BourgPalette.DTOs;

namespace BourgPalette.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public TeamsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.Teams
            .OrderBy(t => t.Id)
            .Select(t => new TeamListItemDto(t.Id, t.Name, t.Description, t.Owner))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var t = await _db.Teams
            .Include(x => x.TeamPokemons)
            .ThenInclude(tp => tp.Pokemon)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return NotFound();

        var members = t.TeamPokemons
            .OrderBy(tp => tp.Slot)
            .Select(tp => new { tp.TeamId, tp.PokemonId, tp.Slot })
            .ToList();
        return Ok(new { t.Id, t.Name, t.Description, t.Owner, Members = members });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TeamCreateDto input, CancellationToken ct)
    {
        var e = new Team { Name = input.Name, Description = input.Description, Owner = input.Owner };
        _db.Teams.Add(e);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, new { id = e.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TeamUpdateDto input, CancellationToken ct)
    {
        var e = await _db.Teams.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        e.Name = input.Name;
        e.Description = input.Description;
        e.Owner = input.Owner;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var e = await _db.Teams.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        _db.Teams.Remove(e);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id });
    }

    // Members management
    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> AddMember([FromRoute] int id, [FromBody] TeamPokemonAddDto input, CancellationToken ct)
    {
        var team = await _db.Teams.Include(t => t.TeamPokemons).FirstOrDefaultAsync(t => t.Id == id, ct);
        if (team is null) return NotFound();

        var pokemonExists = await _db.Pokemons.AnyAsync(p => p.Id == input.PokemonId, ct);
        if (!pokemonExists) return ValidationProblem("Invalid PokemonId");

        if (team.TeamPokemons.Any(tp => tp.Slot == input.Slot))
            return Conflict(new ProblemDetails { Title = "Slot already occupied" });

        var member = new TeamPokemon { TeamId = id, PokemonId = input.PokemonId, Slot = input.Slot };
        _db.TeamPokemons.Add(member);
        await _db.SaveChangesAsync(ct);
    return Ok(new { member.TeamId, member.PokemonId, member.Slot });
    }

    [HttpDelete("{id:int}/members/{pokemonId:int}")]
    public async Task<IActionResult> RemoveMember([FromRoute] int id, [FromRoute] int pokemonId, CancellationToken ct)
    {
        var member = await _db.TeamPokemons.FirstOrDefaultAsync(tp => tp.TeamId == id && tp.PokemonId == pokemonId, ct);
        if (member is null) return NotFound();
        _db.TeamPokemons.Remove(member);
        await _db.SaveChangesAsync(ct);
        return Ok(new { teamId = id, pokemonId });
    }
}