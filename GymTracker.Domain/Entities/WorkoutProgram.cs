using GymTracker.Domain.Common;
using GymTracker.Domain.Enums;

namespace GymTracker.Domain.Entities;

public class WorkoutProgram : BaseEntity
{
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public Guid? GymId { get; set; } // setat cand Visibility = Gym
    public Gym? Gym { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ProgramVisibility Visibility { get; set; } = ProgramVisibility.Private;

    public ICollection<ProgramExercise> Exercises { get; set; } = new List<ProgramExercise>();
}

public class ProgramExercise
{
    public long Id { get; set; }
    public Guid ProgramId { get; set; }
    public WorkoutProgram Program { get; set; } = null!;

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int TargetSets { get; set; } = 3;
    public int TargetReps { get; set; } = 10;
    public int OrderIndex { get; set; }
}
