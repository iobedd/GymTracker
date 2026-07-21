namespace GymTracker.Domain.Common;

/// <summary>
/// Toate entitatile care suporta soft delete + audit mostenesc din asta.
/// Nu se sterge nimic fizic (IsDeleted = 1); randurile sunt filtrate din query-uri
/// prin global query filters (vezi AppDbContext), iar modificarile sunt logate in AuditLogs.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
