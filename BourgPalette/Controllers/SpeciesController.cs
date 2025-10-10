using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BourgPalette.Data;
using BourgPalette.Models;
using BourgPalette.DTOs;
using BourgPalette.Metrics;

namespace BourgPalette.Controllers;

[ApiController]
[Route("api/species")]
[Authorize]
public class SpeciesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SpeciesController> _logger;

    public SpeciesController(ApplicationDbContext db, ILogger<SpeciesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 200) pageSize = 50;

        var query = _db.Species.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                (s.NameEn != null && s.NameEn.ToLower().Contains(term)) ||
                (s.NameFr != null && s.NameFr.ToLower().Contains(term)) ||
                (s.Region != null && s.Region.ToLower().Contains(term))
            );
        }

        var total = await query.CountAsync(ct);
    AppMetrics.NumberTotalSpecies.Inc(total);
        var items = await query
            .OrderBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SpeciesListItemDto(s.Id, s.PokedexNumber, s.NameFr, s.NameEn, s.Generation, s.Region))
            .ToListAsync(ct);

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
    {
        var s = await _db.Species.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();
        var dto = new SpeciesListItemDto(s.Id, s.PokedexNumber, s.NameFr, s.NameEn, s.Generation, s.Region);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SpeciesCreateDto input, CancellationToken ct = default)
    {
        var e = new Species
        {
            PokedexNumber = input.PokedexNumber,
            NameFr = input.NameFr,
            NameEn = input.NameEn,
            Generation = input.Generation,
            Region = input.Region
        };
        _db.Species.Add(e);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, new { id = e.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SpeciesUpdateDto input, CancellationToken ct = default)
    {
        var e = await _db.Species.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        e.PokedexNumber = input.PokedexNumber;
        e.NameFr = input.NameFr;
        e.NameEn = input.NameEn;
        e.Generation = input.Generation;
        e.Region = input.Region;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
    {
        var e = await _db.Species.FindAsync(new object[] { id }, ct);
        if (e is null) return NotFound();
        _db.Species.Remove(e);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id });
    }
}