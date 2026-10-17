using System.Security.Claims;

namespace Pisco.Services;

public static class ClaimsPrincipalExtensions
{
    public static long UserId(this ClaimsPrincipal user) =>
        long.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole("Admin");
}
