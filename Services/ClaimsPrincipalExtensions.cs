using System.Security.Claims;
using Pisco.Domain;

namespace Pisco.Services;

public static class ClaimsPrincipalExtensions
{
    public static bool IsSignedIn(this ClaimsPrincipal user) => user.Identity?.IsAuthenticated == true;

    public static void EnsureSignedIn(this ClaimsPrincipal user)
    {
        if (!user.IsSignedIn())
            throw new DomainException("Domain_SignInRequired");
    }
}
