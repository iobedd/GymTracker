namespace GymTracker.Application.Interfaces;

/// <summary>Info despre userul autentificat curent, populata din claims-urile tokenului Keycloak
/// + din randul local de Users (creat automat prin JIT provisioning).</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }           // Id-ul local (Users.Id), NU "sub"-ul Keycloak
    string? ExternalIdentityId { get; } // claim "sub"
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    Guid? GymId { get; }

    bool IsInRole(string role);
}

/// <summary>Creeaza/actualizeaza userul local pe baza claims-urilor din tokenul Keycloak (JIT provisioning).</summary>
public interface IUserProvisioningService
{
    Task<Guid> EnsureUserProvisionedAsync(System.Security.Claims.ClaimsPrincipal principal, CancellationToken ct = default);
}
