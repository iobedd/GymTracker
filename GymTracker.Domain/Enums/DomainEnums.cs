namespace GymTracker.Domain.Enums;

public enum MachineStatus : byte
{
    Active = 0,
    Maintenance = 1,
    Disabled = 2
}

public enum Difficulty : byte
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2
}

public enum SessionStatus : byte
{
    InProgress = 0,
    Completed = 1,
    Cancelled = 2
}

public enum IssueStatus : byte
{
    Open = 0,
    InProgress = 1,
    Resolved = 2
}

public enum ProgramVisibility : byte
{
    Private = 0, // vizibil doar pentru creator
    Gym = 1,     // vizibil pentru toti clientii salii respective
    Public = 2   // vizibil pe toata platforma, indiferent de sala
}

/// <summary>
/// Rolurile aplicatiei sunt definite si administrate in Keycloak (realm roles).
/// Enumul e folosit doar ca referinta in cod: [Authorize(Roles = nameof(AppRole.Admin))].
/// </summary>
public enum AppRole
{
    Admin,
    Employee,
    Client
}

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
    Restored
}
