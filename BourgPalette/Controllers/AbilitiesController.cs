using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BourgPalette.Data;
using BourgPalette.Models;
using BourgPalette.DTOs;

namespace BourgPalette.Controllers;

[ApiController]
[Route("api/abilities")]
[Authorize]
public class AbilitiesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public AbilitiesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.Abilities
            .OrderBy(a => a.Id)
            .Select(a => new AbilityListItemDto(a.Id, a.Name, a.Description))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var a = await _db.Abilities.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return NotFound();
        return Ok(new AbilityListItemDto(a.Id, a.Name, a.Description));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AbilityCreateDto input, CancellationToken ct)
    {
        var e = new Ability { Name = input.Name, Description = input.Description };
        _db.Abilities.Add(e);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, new { id = e.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AbilityUpdateDto input, CancellationToken ct)
    {
        var e = await _db.Abilities.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        e.Name = input.Name; e.Description = input.Description;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var e = await _db.Abilities.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        _db.Abilities.Remove(e);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id });
    }
}