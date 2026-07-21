using GymTracker.Application.Interfaces;
using GymTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.API.Middleware;

/// <summary>
/// Ruleaza dupa UseAuthentication(): daca request-ul e autentificat, asigura ca exista
/// un rand local in Users (JIT provisioning) si pune Id-ul + GymId local in HttpContext.Items,
/// de unde le citeste CurrentUserService (fara sa mai facem query-uri repetate in fiecare controller).
/// </summary>
public class UserProvisioningMiddleware
{
    private readonly RequestDelegate _next;

    public UserProvisioningMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IUserProvisioningService provisioning, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var localUserId = await provisioning.EnsureUserProvisionedAsync(context.User, context.RequestAborted);
            context.Items["LocalUserId"] = localUserId;

            var gymId = await db.Users.Where(u => u.Id == localUserId).Select(u => u.GymId).FirstOrDefaultAsync();
            context.Items["LocalUserGymId"] = gymId;
        }

        await _next(context);
    }
}
