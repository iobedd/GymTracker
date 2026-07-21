using GymTracker.Domain.Common;
using GymTracker.Domain.Enums;

namespace GymTracker.Domain.Entities;

public class WorkoutSession : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GymId { get; set; }
    public Gym Gym { get; set; } = null!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // calculat la Completed: (CompletedAt - StartedAt) minus suma pauzelor
    public int? DurationSeconds { get; set; }

    public decimal? TotalVolume { get; set; }
    public int? CaloriesBurned { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.InProgress;

    public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
    public ICollection<WorkoutPause> Pauses { get; set; } = new List<WorkoutPause>();
}

/// <summary>O sesiune poate avea oricate pauze (nu doar una) - fiecare pauza e un rand separat.</summary>
public class WorkoutPause
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public WorkoutSession Session { get; set; } = null!;

    public DateTime PausedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResumedAt { get; set; }
}

public class WorkoutSet
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public WorkoutSession Session { get; set; } = null!;

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public Guid? MachineId { get; set; }
    public Machine? Machine { get; set; }

    public int SetNumber { get; set; }
    public decimal Weight { get; set; }
    public int Reps { get; set; }
    public decimal? Rpe { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

public class PersonalRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public decimal Weight { get; set; }
    public int Reps { get; set; }
    public long? WorkoutSetId { get; set; }
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
