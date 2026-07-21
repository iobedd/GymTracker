using GymTracker.Domain.Common;
using GymTracker.Domain.Enums;

namespace GymTracker.Domain.Entities;

public class Machine : BaseEntity
{
    public Guid GymId { get; set; }
    public Gym Gym { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // cod textual unic (ex: "OXY-001"). QR-ul e generat din acest cod pe front-end,
    // nu stocam imaginea - daca se schimba formatul QR, nu trebuie migrata baza de date.
    public string MachineCode { get; set; } = null!;

    public MachineStatus Status { get; set; } = MachineStatus.Active;

    public ICollection<MachineMuscleGroup> MuscleGroups { get; set; } = new List<MachineMuscleGroup>();
    public ICollection<MachineMedia> Media { get; set; } = new List<MachineMedia>();
    public ICollection<ExerciseMachine> Exercises { get; set; } = new List<ExerciseMachine>();
}

public class MachineMuscleGroup
{
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    public int MuscleGroupId { get; set; }
    public MuscleGroup MuscleGroup { get; set; } = null!;
}
