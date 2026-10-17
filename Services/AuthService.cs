using System.Security.Claims;
using System.Security.Cryptography;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
using Pisco.Domain;

namespace Pisco.Services;

public class AuthService(NpgsqlDataSource db)
{
    private const int Iterations = 100_000;
    private const string UserColumns = "id, username, password_hash, password_salt, is_admin, created_at, last_workspace_id";

    public async Task<User?> GetUserAsync(long id)
    {
        await using var conn = await db.OpenConnectionAsync();
        return await conn.QuerySingleOrDefaultAsync<User>(
            $"SELECT {UserColumns} FROM core.users WHERE id = @id", new { id });
    }

    public async Task CreateAdminAsync(string username, string password)
    {
        var user = new User { Username = username, IsAdmin = true };
        SetPassword(user, password);

        await using var conn = await db.OpenConnectionAsync();
        await conn.ExecuteAsync(
            "INSERT INTO core.users (username, password_hash, password_salt, is_admin) VALUES (@Username, @PasswordHash, @PasswordSalt, @IsAdmin)",
            user);
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        username = username.Trim();
        if (username.Length == 0 || password.Length == 0)
            return null;

        await using var conn = await db.OpenConnectionAsync();
        var user = await conn.QuerySingleOrDefaultAsync<User>(
            $"SELECT {UserColumns} FROM core.users WHERE username = @username", new { username });
        return user is not null && CryptographicOperations.FixedTimeEquals(Hash(password, user.PasswordSalt), user.PasswordHash)
            ? user
            : null;
    }

    public Task SignInAsync(HttpContext http, User user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
        ];
        if (user.IsAdmin)
            claims.Add(new(ClaimTypes.Role, "Admin"));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        return http.SignInAsync(principal, new AuthenticationProperties { IsPersistent = true });
    }

    private static void SetPassword(User user, string password)
    {
        user.PasswordSalt = RandomNumberGenerator.GetBytes(16);
        user.PasswordHash = Hash(password, user.PasswordSalt);
    }

    private static byte[] Hash(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
}
