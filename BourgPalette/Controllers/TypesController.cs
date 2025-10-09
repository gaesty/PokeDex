using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BourgPalette.Data;
using BourgPalette.Models;
using BourgPalette.DTOs;

namespace BourgPalette.Controllers;

[ApiController]
[Route("api/types")]
[Authorize]
public class TypesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public TypesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.Types
            .OrderBy(t => t.Id)
            .Select(t => new TypeListItemDto(t.Id, t.Name, t.Description))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var t = await _db.Types.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return NotFound();
        return Ok(new TypeListItemDto(t.Id, t.Name, t.Description));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TypeCreateDto input, CancellationToken ct)
    {
        var e = new TypeDefinition { Name = input.Name, Description = input.Description };
        _db.Types.Add(e);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, new { id = e.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TypeUpdateDto input, CancellationToken ct)
    {
        var e = await _db.Types.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        e.Name = input.Name; e.Description = input.Description;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var e = await _db.Types.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        _db.Types.Remove(e);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id });
    }
}