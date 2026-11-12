using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Pisco.Services;

public class AuthService(ConfigService config)
{
    public async Task<bool> SignInAsync(HttpContext http, string username, string password)
    {
        if (!await config.ValidateCredentialsAsync(username, password))
            return false;
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, username.Trim())], CookieAuthenticationDefaults.AuthenticationScheme);
        await http.SignInAsync(new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true });
        return true;
    }
}
