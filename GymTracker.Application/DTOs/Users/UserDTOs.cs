namespace GymTracker.Application.DTOs.Users;

/// <summary>Profilul userului curent, asa cum e vazut de FE dupa login prin Keycloak.</summary>
public class MeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Guid? GymId { get; set; }
    public List<string> Roles { get; set; } = new(); // extrase din tokenul Keycloak
}
