using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymTracker.Application.DTOs.Gyms;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Data;

namespace GymTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GymsController : ControllerBase
{
    private readonly AppDbContext _db;
    public GymsController(AppDbContext db) => _db = db;

    // public: userii aleg sala din lista la inregistrare/asociere
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<GymDto>>> GetAll()
    {
        var gyms = await _db.Gyms.OrderBy(g => g.Name).Select(g => ToDto(g)).ToListAsync();
        return Ok(gyms);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<GymDto>> GetById(Guid id)
    {
        var gym = await _db.Gyms.FindAsync(id);
        if (gym is null) return NotFound();
        return Ok(ToDto(gym));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<GymDto>> Create(CreateGymRequest request)
    {
        var slug = await GenerateUniqueSlug(request.Name);

        var gym = new Gym
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Address = request.Address,
            City = request.City,
            Phone = request.Phone
        };

        _db.Gyms.Add(gym);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = gym.Id }, ToDto(gym));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, UpdateGymRequest request)
    {
        var gym = await _db.Gyms.FindAsync(id);
        if (gym is null) return NotFound();

        gym.Name = request.Name.Trim();
        gym.Address = request.Address;
        gym.City = request.City;
        gym.Phone = request.Phone;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var gym = await _db.Gyms.FindAsync(id);
        if (gym is null) return NotFound();

        _db.Gyms.Remove(gym); // soft delete automat (vezi AppDbContext.SaveChanges)
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var gym = await _db.Gyms.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Id == id);
        if (gym is null) return NotFound();

        gym.IsDeleted = false;
        gym.DeletedAt = null;
        gym.DeletedBy = null;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static GymDto ToDto(Gym g) => new()
    {
        Id = g.Id, Name = g.Name, Address = g.Address, City = g.City, Phone = g.Phone
    };

    private static string GenerateBaseSlug(string name)
    {
        return name
            .Trim()
            .ToUpper()
            .Replace(" ", "-");
    }

    private async Task<string> GenerateUniqueSlug(string name)
    {
        var baseSlug = GenerateBaseSlug(name);
        var slug = baseSlug;

        int counter = 1;

        while (await _db.Gyms.AnyAsync(g => g.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
