using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymTracker.Infrastructure.Data;

namespace GymTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MuscleGroupsController : ControllerBase
{
    private readonly AppDbContext _db;
    public MuscleGroupsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _db.MuscleGroups.OrderBy(g => g.Name).Select(g => new { g.Id, g.Name }).ToListAsync();
        return Ok(groups);
    }
}
