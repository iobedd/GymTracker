using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymTracker.Application.DTOs.Users;
using GymTracker.Application.Interfaces;
using GymTracker.Infrastructure.Data;

namespace GymTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UsersController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // GET /api/users/me
    // Userul local e deja creat automat de UserProvisioningMiddleware la acest punct.
    // FE apeleaza asta imediat dupa login (Keycloak) ca sa afle profilul + rolurile.
    [HttpGet("me")]
    public async Task<ActionResult<MeDto>> GetMe()
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId);
        if (user is null) return NotFound();

        return Ok(new MeDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            GymId = user.GymId,
            Roles = _currentUser.Roles.ToList()
        });
    }

    // useri isi pot alege / schimba sala "de baza" dupa inregistrare
    [HttpPut("me/gym/{gymId:guid}")]
    public async Task<IActionResult> SetMyGym(Guid gymId)
    {
        var gymExists = await _db.Gyms.AnyAsync(g => g.Id == gymId);
        if (!gymExists) return NotFound(new { message = "Sala nu exista." });

        var user = await _db.Users.FirstAsync(u => u.Id == _currentUser.UserId);
        user.GymId = gymId;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
