using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BourgPalette.Data;
using BourgPalette.Models;
using BourgPalette.DTOs;

namespace BourgPalette.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public MediaController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.Media
            .OrderBy(m => m.Id)
            .Select(m => new MediaListItemDto(m.Id, m.SpriteUrl, m.ArtworkUrl, m.Note))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var m = await _db.Media.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return NotFound();
        return Ok(new MediaListItemDto(m.Id, m.SpriteUrl, m.ArtworkUrl, m.Note));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MediaCreateDto input, CancellationToken ct)
    {
        var e = new Media { SpriteUrl = input.SpriteUrl, ArtworkUrl = input.ArtworkUrl, Note = input.Note };
        _db.Media.Add(e);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, new { id = e.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] MediaUpdateDto input, CancellationToken ct)
    {
        var e = await _db.Media.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        e.SpriteUrl = input.SpriteUrl; e.ArtworkUrl = input.ArtworkUrl; e.Note = input.Note;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var e = await _db.Media.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        _db.Media.Remove(e);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id });
    }
}