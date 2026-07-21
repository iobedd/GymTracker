using GymTracker.Domain.Common;

namespace GymTracker.Domain.Entities;

/// <summary>Un tenant logic. Aplicatia e gandita multi-gym: prima instanta va avea o singura sala (Oxygen),
/// dar orice alta sala poate fi adaugata fara modificari de schema.</summary>
public class Gym : BaseEntity
{
    public string Name { get; set; } = null!;

    // cod scurt unic, folosit ca prefix la generarea MachineCode (ex: "OXY" -> "OXY-001")
    public string Slug { get; set; } = null!;

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }

    public ICollection<Machine> Machines { get; set; } = new List<Machine>();
}
