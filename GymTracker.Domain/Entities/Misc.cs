using GymTracker.Domain.Enums;

namespace GymTracker.Domain.Entities;

public class MachineReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public byte Rating { get; set; } // 1-5
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

public class MachineIssueReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    public Guid ReportedByUserId { get; set; }
    public User ReportedByUser { get; set; } = null!;

    public string Description { get; set; } = null!;
    public IssueStatus Status { get; set; } = IssueStatus.Open;
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
}

/// <summary>ReadAt in loc de IsRead: pastram si informatia CAND a fost citita notificarea.</summary>
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime? ReadAt { get; set; } // NULL = necitita
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Favorite tipizate (nu generic EntityType/EntityId) - avem FK reale si integritate referentiala.

public class FavoriteMachine
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FavoriteExercise
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FavoriteProgram
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ProgramId { get; set; }
    public WorkoutProgram Program { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class GymAttendance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GymId { get; set; }
    public Gym Gym { get; set; } = null!;

    public DateTime CheckInAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutAt { get; set; }
}

/// <summary>Tabel generic de audit: inregistreaza Create/Update/Delete/Restore pentru orice entitate.
/// Populat automat din AppDbContext.SaveChanges (vezi Infrastructure), nu manual din controllere.</summary>
public class AuditLog
{
    public long Id { get; set; }
    public string EntityName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public AuditAction Action { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
}
