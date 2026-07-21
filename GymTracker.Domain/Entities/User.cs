using GymTracker.Domain.Common;

namespace GymTracker.Domain.Entities;

/// <summary>
/// Fara PasswordHash si fara Role local: autentificarea si rolurile sunt gestionate de Keycloak.
/// Randul e creat automat (JIT provisioning) la primul request autentificat cu succes,
/// pe baza claim-ului "sub" din tokenul JWT (ExternalIdentityId).
/// </summary>
public class User : BaseEntity
{
    public string ExternalIdentityId { get; set; } = null!; // Keycloak "sub"

    public Guid? GymId { get; set; }
    public Gym? Gym { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
}
