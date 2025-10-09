using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BourgPalette.Data;
using BourgPalette.Models;
using BourgPalette.DTOs;

namespace BourgPalette.Controllers;

[ApiController]
[Route("api/moves")]
[Authorize]
public class MovesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public MovesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.Moves
            .OrderBy(m => m.Id)
            .Select(m => new MoveListItemDto(
                m.Id,
                m.Name,
                m.Description,
                m.TypeId,
                m.Power,
                m.Accuracy,
                m.PP,
                m.Category))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var m = await _db.Moves.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return NotFound();
        return Ok(new MoveListItemDto(
            m.Id,
            m.Name,
            m.Description,
            m.TypeId,
            m.Power,
            m.Accuracy,
            m.PP,
            m.Category));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MoveCreateDto input, CancellationToken ct)
    {
        if (input.TypeId.HasValue)
        {
            var typeExists = await _db.Types.AnyAsync(t => t.Id == input.TypeId, ct);
            if (!typeExists) return ValidationProblem("Invalid TypeId");
        }
        var e = new Move
        {
            Name = input.Name,
            Description = input.Description,
            TypeId = input.TypeId,
            Power = input.Power,
            Accuracy = input.Accuracy,
            PP = input.PP,
            Category = input.Category
        };
        _db.Moves.Add(e);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, new { id = e.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] MoveUpdateDto input, CancellationToken ct)
    {
        var e = await _db.Moves.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        if (input.TypeId != e.TypeId)
        {
            if (input.TypeId.HasValue)
            {
                var typeExists = await _db.Types.AnyAsync(t => t.Id == input.TypeId, ct);
                if (!typeExists) return ValidationProblem("Invalid TypeId");
            }
            e.TypeId = input.TypeId;
        }
        e.Name = input.Name;
        e.Description = input.Description;
        e.Power = input.Power;
        e.Accuracy = input.Accuracy;
        e.PP = input.PP;
        e.Category = input.Category;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var e = await _db.Moves.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        _db.Moves.Remove(e);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id });
    }
}