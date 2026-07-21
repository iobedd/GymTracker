using System.ComponentModel.DataAnnotations;
using GymTracker.Application.DTOs.Common;
using GymTracker.Domain.Enums;

namespace GymTracker.Application.DTOs.Machines;

public class MachineDto
{
    public Guid Id { get; set; }
    public Guid GymId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string MachineCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public List<string> MuscleGroups { get; set; } = new();
}

public class MachineQueryParameters : QueryParameters
{
    public Guid? GymId { get; set; }
    public string? MuscleGroup { get; set; } // ex: "Chest"
    public MachineStatus? Status { get; set; }
}

public class CreateMachineRequest
{
    [Required]
    public Guid GymId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public List<int> MuscleGroupIds { get; set; } = new();
}

public class UpdateMachineRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public MachineStatus Status { get; set; }

    [Required]
    public List<int> MuscleGroupIds { get; set; } = new();
}

public class ReportIssueRequest
{
    [Required, MaxLength(1000)]
    public string Description { get; set; } = null!;
}
