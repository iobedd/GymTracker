using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GymTracker.Application.Interfaces;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Data;

namespace GymTracker.Infrastructure.Services;

/// <summary>
/// JIT (just-in-time) provisioning: la primul request autentificat cu succes prin Keycloak,
/// creeaza automat randul local din Users pe baza claim-urilor din token.
/// La request-urile urmatoare doar il gaseste dupa ExternalIdentityId ("sub").
/// </summary>
public class UserProvisioningService : IUserProvisioningService
{
    private readonly AppDbContext _db;

    public UserProvisioningService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> EnsureUserProvisionedAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var externalId = principal.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Tokenul nu contine claim-ul 'sub'.");

        var existing = await _db.Users.FirstOrDefaultAsync(u => u.ExternalIdentityId == externalId, ct);
        if (existing is not null)
            return existing.Id;

        var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email") ?? $"{externalId}@unknown.local";
        var firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.FindFirstValue("given_name") ?? "Nume";
        var lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? principal.FindFirstValue("family_name") ?? "Necunoscut";

        var user = new User
        {
            ExternalIdentityId = externalId,
            Email = email.ToLower(),
            FirstName = firstName,
            LastName = lastName
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return user.Id;
    }
}
