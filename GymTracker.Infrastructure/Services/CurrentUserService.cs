using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using GymTracker.Application.Interfaces;

namespace GymTracker.Infrastructure.Services;

/// <summary>
/// Expune userul curent pornind de la claims-urile din tokenul Keycloak + Id-ul local
/// (populat de UserProvisioningMiddleware in HttpContext.Items dupa JIT provisioning).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.Items["LocalUserId"] as Guid?;
            return raw;
        }
    }

    public string? ExternalIdentityId => Principal?.FindFirstValue("sub");

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email) ?? Principal?.FindFirstValue("email");

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public Guid? GymId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.Items["LocalUserGymId"] as Guid?;
            return raw;
        }
    }

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
