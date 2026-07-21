using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using GymTracker.Application.Interfaces;
using GymTracker.Domain.Common;
using GymTracker.Domain.Entities;
using GymTracker.Domain.Enums;

namespace GymTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    // constructor fara ICurrentUserService (folosit de EF Core tooling / migrations)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MuscleGroup> MuscleGroups => Set<MuscleGroup>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<MachineMuscleGroup> MachineMuscleGroups => Set<MachineMuscleGroup>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseMuscleGroup> ExerciseMuscleGroups => Set<ExerciseMuscleGroup>();
    public DbSet<ExerciseMachine> ExerciseMachines => Set<ExerciseMachine>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<ExerciseMedia> ExerciseMedia => Set<ExerciseMedia>();
    public DbSet<MachineMedia> MachineMedia => Set<MachineMedia>();
    public DbSet<WorkoutProgram> WorkoutPrograms => Set<WorkoutProgram>();
    public DbSet<ProgramExercise> ProgramExercises => Set<ProgramExercise>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutPause> WorkoutPauses => Set<WorkoutPause>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<PersonalRecord> PersonalRecords => Set<PersonalRecord>();
    public DbSet<MachineReview> MachineReviews => Set<MachineReview>();
    public DbSet<MachineIssueReport> MachineIssueReports => Set<MachineIssueReport>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FavoriteMachine> FavoriteMachines => Set<FavoriteMachine>();
    public DbSet<FavoriteExercise> FavoriteExercises => Set<FavoriteExercise>();
    public DbSet<FavoriteProgram> FavoritePrograms => Set<FavoriteProgram>();
    public DbSet<GymAttendance> GymAttendances => Set<GymAttendance>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MuscleGroup>().HasData(
            new MuscleGroup { Id = 1, Name = "Chest" },
            new MuscleGroup { Id = 2, Name = "Back" },
            new MuscleGroup { Id = 3, Name = "Shoulders" },
            new MuscleGroup { Id = 4, Name = "Arms" },
            new MuscleGroup { Id = 5, Name = "Legs" },
            new MuscleGroup { Id = 6, Name = "Glutes" },
            new MuscleGroup { Id = 7, Name = "Core" }
        );

        modelBuilder.Entity<Gym>().HasIndex(g => g.Slug).IsUnique();

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.ExternalIdentityId).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Gym).WithMany().HasForeignKey(u => u.GymId).OnDelete(DeleteBehavior.SetNull);
        });

        // ---- Machine <-> MuscleGroup ----
        modelBuilder.Entity<MachineMuscleGroup>(e =>
        {
            e.HasKey(mm => new { mm.MachineId, mm.MuscleGroupId });
            e.HasOne(mm => mm.Machine).WithMany(m => m.MuscleGroups).HasForeignKey(mm => mm.MachineId);
            e.HasOne(mm => mm.MuscleGroup).WithMany().HasForeignKey(mm => mm.MuscleGroupId);
        });

        modelBuilder.Entity<Machine>(e =>
        {
            e.HasIndex(m => m.MachineCode).IsUnique();
            e.HasIndex(m => m.GymId);
            e.HasOne(m => m.Gym).WithMany(g => g.Machines).HasForeignKey(m => m.GymId);
        });

        // ---- Exercise <-> MuscleGroup (primar/secundar) ----
        modelBuilder.Entity<ExerciseMuscleGroup>(e =>
        {
            e.HasKey(em => new { em.ExerciseId, em.MuscleGroupId });
            e.HasOne(em => em.Exercise).WithMany(ex => ex.MuscleGroups).HasForeignKey(em => em.ExerciseId);
            e.HasOne(em => em.MuscleGroup).WithMany().HasForeignKey(em => em.MuscleGroupId);
            e.HasIndex(em => em.MuscleGroupId);
        });

        // ---- Exercise <-> Machine ----
        modelBuilder.Entity<ExerciseMachine>(e =>
        {
            e.HasKey(em => new { em.ExerciseId, em.MachineId });
            e.HasOne(em => em.Exercise).WithMany(ex => ex.Machines).HasForeignKey(em => em.ExerciseId);
            e.HasOne(em => em.Machine).WithMany(m => m.Exercises).HasForeignKey(em => em.MachineId);
        });

        // ---- Media ----
        modelBuilder.Entity<ExerciseMedia>(e =>
        {
            e.HasKey(em => new { em.ExerciseId, em.MediaId });
            e.HasOne(em => em.Exercise).WithMany(ex => ex.Media).HasForeignKey(em => em.ExerciseId);
            e.HasOne(em => em.Media).WithMany().HasForeignKey(em => em.MediaId);
        });
        modelBuilder.Entity<MachineMedia>(e =>
        {
            e.HasKey(mm => new { mm.MachineId, mm.MediaId });
            e.HasOne(mm => mm.Machine).WithMany(m => m.Media).HasForeignKey(mm => mm.MachineId);
            e.HasOne(mm => mm.Media).WithMany().HasForeignKey(mm => mm.MediaId);
        });

        // ---- Favorites tipizate ----
        modelBuilder.Entity<FavoriteMachine>().HasIndex(f => new { f.UserId, f.MachineId }).IsUnique();
        modelBuilder.Entity<FavoriteExercise>().HasIndex(f => new { f.UserId, f.ExerciseId }).IsUnique();
        modelBuilder.Entity<FavoriteProgram>().HasIndex(f => new { f.UserId, f.ProgramId }).IsUnique();

        // ---- Notifications ----
        modelBuilder.Entity<Notification>().HasIndex(n => new { n.UserId, n.ReadAt });

        // ---- Decimal precision ----
        modelBuilder.Entity<WorkoutSet>().Property(s => s.Weight).HasPrecision(6, 2);
        modelBuilder.Entity<WorkoutSet>().Property(s => s.Rpe).HasPrecision(3, 1);
        modelBuilder.Entity<PersonalRecord>().Property(p => p.Weight).HasPrecision(6, 2);
        modelBuilder.Entity<WorkoutSession>().Property(s => s.TotalVolume).HasPrecision(10, 2);

        // ---- Indecsi suplimentari ceruti explicit ----
        modelBuilder.Entity<WorkoutSession>().HasIndex(s => s.UserId);
        modelBuilder.Entity<WorkoutSet>().HasIndex(s => s.SessionId);
        modelBuilder.Entity<PersonalRecord>().HasIndex(p => p.UserId);
        modelBuilder.Entity<MachineIssueReport>().HasIndex(r => r.Status);
        modelBuilder.Entity<GymAttendance>().HasIndex(a => a.UserId);
        modelBuilder.Entity<AuditLog>().HasIndex(a => new { a.EntityName, a.EntityId });

        // ---- SOFT DELETE: global query filters ----
        modelBuilder.Entity<Gym>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Machine>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Exercise>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<WorkoutProgram>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<WorkoutSession>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<MachineReview>().HasQueryFilter(x => !x.IsDeleted);
    }

    public override int SaveChanges()
    {
        var auditEntries = CaptureAuditEntries();
        var result = base.SaveChanges();
        PersistAudit(auditEntries);
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = CaptureAuditEntries();
        var result = await base.SaveChangesAsync(cancellationToken);
        await PersistAuditAsync(auditEntries, cancellationToken);
        return result;
    }

    /// <summary>
    /// Transforma orice Remove() pe o entitate cu soft delete intr-un UPDATE (IsDeleted = 1),
    /// si pregateste inregistrari de audit (Created/Updated/Deleted) pentru toate entitatile urmarite.
    /// Audit-ul e generic - inlocuieste orice logare manuala facuta din controllere.
    /// </summary>
    private List<AuditLog> CaptureAuditEntries()
    {
        var entries = new List<AuditLog>();
        var userId = _currentUser?.UserId;

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is AuditLog) continue;

            AuditAction? action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted when entry.Entity is BaseEntity => AuditAction.Deleted,
                _ => null
            };

            if (entry.State == EntityState.Deleted && entry.Entity is BaseEntity softDeletable)
            {
                // soft delete: nu stergem randul, doar il marcam
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTime.UtcNow;
                softDeletable.DeletedBy = userId;
            }
            else if (entry.State == EntityState.Modified && entry.Entity is BaseEntity auditable)
            {
                auditable.UpdatedAt = DateTime.UtcNow;
            }

            if (action is null) continue;

            var entityName = entry.Entity.GetType().Name;
            var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            var entityId = idProp?.CurrentValue?.ToString() ?? "unknown";

            string? oldValues = action == AuditAction.Updated
                ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue))
                : null;
            string? newValues = action != AuditAction.Deleted
                ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue))
                : null;

            entries.Add(new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action.Value,
                ChangedByUserId = userId,
                OldValues = oldValues,
                NewValues = newValues
            });
        }

        return entries;
    }

    private void PersistAudit(List<AuditLog> entries)
    {
        if (entries.Count == 0) return;
        AuditLogs.AddRange(entries);
        base.SaveChanges();
    }

    private async Task PersistAuditAsync(List<AuditLog> entries, CancellationToken ct)
    {
        if (entries.Count == 0) return;
        AuditLogs.AddRange(entries);
        await base.SaveChangesAsync(ct);
    }
}
