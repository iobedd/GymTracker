using GymTracker.Domain.Common;
using GymTracker.Domain.Enums;

namespace GymTracker.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Difficulty Difficulty { get; set; } = Difficulty.Beginner;

    public ICollection<ExerciseMuscleGroup> MuscleGroups { get; set; } = new List<ExerciseMuscleGroup>();
    public ICollection<ExerciseMachine> Machines { get; set; } = new List<ExerciseMachine>();
    public ICollection<ExerciseMedia> Media { get; set; } = new List<ExerciseMedia>();
}

/// <summary>Un exercitiu poate lucra mai multi muschi, cu distinctie primar/secundar
/// (ex: Bench Press -> Chest=primar, Triceps+FrontDeltoid=secundar).</summary>
public class ExerciseMuscleGroup
{
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int MuscleGroupId { get; set; }
    public MuscleGroup MuscleGroup { get; set; } = null!;

    public bool IsPrimary { get; set; } = true;
}

/// <summary>Many-to-many: pe ce aparate se poate face un exercitiu.</summary>
public class ExerciseMachine
{
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
}
