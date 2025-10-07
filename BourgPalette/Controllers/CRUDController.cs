using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BourgPalette.Data;
using BourgPalette.Models;
using BourgPalette.DTOs;
using PokemonEntity = BourgPalette.Models.Pokemon;

namespace BourgPalette.Controllers
{
    [ApiController]
    [Route("api/pokedex")]
    public class PokedexController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PokedexController> _logger;

        public PokedexController(ApplicationDbContext db, ILogger<PokedexController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET api/pokedex
        [HttpGet]
        public async Task<IActionResult> GetPokedex([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _db.Pokemons
                .Include(p => p.Species)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    (p.Species != null && (
                        (p.Species.NameEn != null && p.Species.NameEn.ToLower().Contains(term)) ||
                        (p.Species.NameFr != null && p.Species.NameFr.ToLower().Contains(term))
                    )) ||
                    (p.Form != null && p.Form.ToLower().Contains(term))
                );
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PokemonListItemDto(
                    p.Id,
                    p.SpeciesId,
                    p.Species != null ? p.Species.PokedexNumber : null,
                    p.Species != null ? p.Species.NameFr : null,
                    p.Species != null ? p.Species.NameEn : null,
                    p.Form,
                    p.Height,
                    p.Weight,
                    p.BaseExperience,
                    p.ImageId
                ))
                .ToListAsync(ct);

            return Ok(new { total, page, pageSize, items });
        }

        // GET api/pokedex/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var p = await _db.Pokemons.Include(x => x.Species).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();
            var dto = new PokemonListItemDto(
                p.Id,
                p.SpeciesId,
                p.Species?.PokedexNumber,
                p.Species?.NameFr,
                p.Species?.NameEn,
                p.Form,
                p.Height,
                p.Weight,
                p.BaseExperience,
                p.ImageId
            );
            return Ok(dto);
        }

        // POST api/pokedex
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PokemonCreateDto input, CancellationToken ct = default)
        {
            var errors = new Dictionary<string, string[]>();
            if (input.SpeciesId is null)
                errors["speciesId"] = new[] { "SpeciesId is required." };
            if (errors.Count > 0) return BadRequest(new { errors });

            if (!await _db.Species.AnyAsync(s => s.Id == input.SpeciesId, ct))
                return BadRequest(new { message = $"Species {input.SpeciesId} does not exist." });

            if (input.ImageId is not null && !await _db.Media.AnyAsync(m => m.Id == input.ImageId, ct))
                return BadRequest(new { message = $"Media {input.ImageId} does not exist." });

            var entity = new PokemonEntity
            {
                SpeciesId = input.SpeciesId,
                Form = input.Form,
                Height = input.Height,
                Weight = input.Weight,
                BaseExperience = input.BaseExperience,
                ImageId = input.ImageId
            };
            _db.Pokemons.Add(entity);
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new { id = entity.Id });
        }

        // PUT api/pokedex/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PokemonUpdateDto input, CancellationToken ct = default)
        {
            var entity = await _db.Pokemons.FindAsync(new object?[] { id }, ct);
            if (entity is null) return NotFound();

            if (input.SpeciesId is null)
                return BadRequest(new { errors = new { speciesId = new[] { "SpeciesId is required." } } });

            if (!await _db.Species.AnyAsync(s => s.Id == input.SpeciesId, ct))
                return BadRequest(new { message = $"Species {input.SpeciesId} does not exist." });

            if (input.ImageId is not null && !await _db.Media.AnyAsync(m => m.Id == input.ImageId, ct))
                return BadRequest(new { message = $"Media {input.ImageId} does not exist." });

            entity.SpeciesId = input.SpeciesId;
            entity.Form = input.Form;
            entity.Height = input.Height;
            entity.Weight = input.Weight;
            entity.BaseExperience = input.BaseExperience;
            entity.ImageId = input.ImageId;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE api/pokedex/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
        {
            var entity = await _db.Pokemons.FindAsync(new object?[] { id }, ct);
            if (entity is null) return NotFound();

            _db.Pokemons.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return Ok(entity);
        }
    }
}
