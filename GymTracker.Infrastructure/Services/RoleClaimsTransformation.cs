using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
namespace GymTracker.Infrastructure.Services;

/// <summary>
/// Keycloak pune rolurile de realm intr-un claim JSON "realm_access": { "roles": ["Admin", ...] },
/// nu ca ClaimTypes.Role individuale. Aceasta clasa "traduce" acel claim in ClaimTypes.Role standard,
/// ca [Authorize(Roles = "Admin")] / RequireRole(...) din ASP.NET Core sa functioneze normal.
/// </summary>
public class RoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity is null || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        // evitam sa adaugam claims duplicate daca transformarea ruleaza de mai multe ori
        if (identity.HasClaim(c => c.Type == ClaimTypes.Role))
            return Task.FromResult(principal);

        var realmAccessJson = identity.FindFirst("realm_access")?.Value;
        if (!string.IsNullOrEmpty(realmAccessJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(realmAccessJson);
                if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (!string.IsNullOrEmpty(roleName))
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
            }
            catch (JsonException)
            {
                // token malformat - ignoram, userul va ramane fara roluri (deci fara acces la endpoint-uri protejate)
            }
        }

        return Task.FromResult(principal);
    }
}
