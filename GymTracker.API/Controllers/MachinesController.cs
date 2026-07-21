using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymTracker.Application.DTOs.Common;
using GymTracker.Application.DTOs.Machines;
using GymTracker.Application.Interfaces;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Data;

namespace GymTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachinesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MachinesController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // GET /api/machines?page=2&pageSize=15&muscleGroup=Chest&status=Active&search=leg&sort=name
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResult<MachineDto>>> GetAll([FromQuery] MachineQueryParameters query)
    {
        var machinesQuery = _db.Machines
            .Include(m => m.MuscleGroups).ThenInclude(mg => mg.MuscleGroup)
            .AsQueryable();

        if (query.GymId.HasValue)
            machinesQuery = machinesQuery.Where(m => m.GymId == query.GymId);

        if (query.Status.HasValue)
            machinesQuery = machinesQuery.Where(m => m.Status == query.Status);

        if (!string.IsNullOrWhiteSpace(query.MuscleGroup))
            machinesQuery = machinesQuery.Where(m => m.MuscleGroups.Any(mg => mg.MuscleGroup.Name == query.MuscleGroup));

        if (!string.IsNullOrWhiteSpace(query.Search))
            machinesQuery = machinesQuery.Where(m => m.Name.Contains(query.Search));

        machinesQuery = query.Sort switch
        {
            "name" => machinesQuery.OrderBy(m => m.Name),
            "-name" => machinesQuery.OrderByDescending(m => m.Name),
            "createdAt" => machinesQuery.OrderBy(m => m.CreatedAt),
            "-createdAt" => machinesQuery.OrderByDescending(m => m.CreatedAt),
            _ => machinesQuery.OrderBy(m => m.Name)
        };

        var totalCount = await machinesQuery.CountAsync();
        var items = await machinesQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => ToDto(m))
            .ToListAsync();

        return Ok(new PagedResult<MachineDto>
        {
            Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = totalCount
        });
    }

    // scanare QR (codul e citit din imaginea QR pe FE si trimis aici ca text)
    [HttpGet("code/{machineCode}")]
    [Authorize]
    public async Task<ActionResult<MachineDto>> GetByCode(string machineCode)
    {
        var machine = await _db.Machines
            .Include(m => m.MuscleGroups).ThenInclude(mg => mg.MuscleGroup)
            .FirstOrDefaultAsync(m => m.MachineCode == machineCode);

        if (machine is null) return NotFound(new { message = "Aparat inexistent pentru acest cod." });
        return Ok(ToDto(machine));
    }

    [HttpPost]
    [Authorize(Policy = "EmployeeOrAdmin")]
    public async Task<ActionResult<MachineDto>> Create(CreateMachineRequest request)
    {
        var gym = await _db.Gyms.FindAsync(request.GymId);
        if (gym is null) return BadRequest(new { message = "Sala specificata nu exista." });

        // MachineCode = <SlugSala>-<secventa>, ex: OXY-001, OXY-002...
        var countForGym = await _db.Machines.IgnoreQueryFilters().CountAsync(m => m.GymId == request.GymId);
        var machineCode = $"{gym.Slug}-{(countForGym + 1):D3}";

        var machine = new Machine
        {
            GymId = request.GymId,
            Name = request.Name.Trim(),
            Description = request.Description,
            MachineCode = machineCode
        };

        foreach (var muscleGroupId in request.MuscleGroupIds.Distinct())
            machine.MuscleGroups.Add(new MachineMuscleGroup { MuscleGroupId = muscleGroupId, Machine = machine });

        _db.Machines.Add(machine);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByCode), new { machineCode = machine.MachineCode }, ToDto(machine));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "EmployeeOrAdmin")]
    public async Task<IActionResult> Update(Guid id, UpdateMachineRequest request)
    {
        var machine = await _db.Machines.Include(m => m.MuscleGroups).FirstOrDefaultAsync(m => m.Id == id);
        if (machine is null) return NotFound();

        // Nu mai logam manual istoricul aici - AppDbContext.SaveChanges scrie automat
        // un rand in AuditLogs (Action=Updated, OldValues/NewValues) pentru orice schimbare.
        machine.Name = request.Name.Trim();
        machine.Description = request.Description;
        machine.Status = request.Status;

        machine.MuscleGroups.Clear();
        foreach (var muscleGroupId in request.MuscleGroupIds.Distinct())
            machine.MuscleGroups.Add(new MachineMuscleGroup { MuscleGroupId = muscleGroupId, MachineId = machine.Id });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "EmployeeOrAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var machine = await _db.Machines.FindAsync(id);
        if (machine is null) return NotFound();

        _db.Machines.Remove(machine);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // "red alert button" - raportare defectiune
    [HttpPost("{id:guid}/report-issue")]
    [Authorize]
    public async Task<IActionResult> ReportIssue(Guid id, ReportIssueRequest request)
    {
        var machineExists = await _db.Machines.AnyAsync(m => m.Id == id);
        if (!machineExists) return NotFound();

        _db.MachineIssueReports.Add(new MachineIssueReport
        {
            MachineId = id,
            ReportedByUserId = _currentUser.UserId!.Value,
            Description = request.Description
        });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static MachineDto ToDto(Machine m) => new()
    {
        Id = m.Id,
        GymId = m.GymId,
        Name = m.Name,
        Description = m.Description,
        MachineCode = m.MachineCode,
        Status = m.Status.ToString(),
        MuscleGroups = m.MuscleGroups.Select(mg => mg.MuscleGroup.Name).ToList()
    };
}
