using System.Security.Cryptography;

namespace Pisco.Domain;

public class Config
{
    public const short SingletonId = 1;
    public const int UsernameLengthMax = 40;
    public const int PasswordLengthMin = 4;

    private const int iterations = 100_000;
    private const int saltSize = 16;
    private const int hashSize = 32;

    public short Id { get; private set; }
    public string Username { get; private set; } = "";
    public byte[] PasswordHash { get; private set; } = [];
    public byte[] PasswordSalt { get; private set; } = [];
    public long? LandingWorkspaceId { get; private set; }
    public long? LastWorkspaceId { get; private set; }

    private Config() { }

    public Config(string username, string password)
    {
        Id = SingletonId;
        SetUsername(username);
        SetPassword(password);
    }

    public void SetUsername(string username)
    {
        username = (username ?? "").Trim();
        if (username.Length == 0)
            throw new DomainException("Domain_Name_Empty");
        if (username.Length > UsernameLengthMax)
            throw new DomainException("Domain_Name_TooLong", UsernameLengthMax);
        Username = username;
    }

    public void SetPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < PasswordLengthMin)
            throw new DomainException("Domain_Config_PasswordTooShort", PasswordLengthMin);
        PasswordSalt = RandomNumberGenerator.GetBytes(saltSize);
        PasswordHash = hash(password, PasswordSalt);
    }

    public bool CheckCredentials(string username, string password) =>
        string.Equals((username ?? "").Trim(), Username, StringComparison.Ordinal)
        && !string.IsNullOrEmpty(password)
        && CryptographicOperations.FixedTimeEquals(hash(password, PasswordSalt), PasswordHash);

    public bool IsLanding(Workspace ws) => ws.Id == LandingWorkspaceId;

    public void SetLanding(Workspace ws)
    {
        if (!ws.IsPublic)
            throw new DomainException("Domain_Config_LandingMustBePublic");
        LandingWorkspaceId = ws.Id;
    }

    public void SetLastWorkspace(long? workspaceId) => LastWorkspaceId = workspaceId;

    public void EnsureCanMakePrivate(Workspace ws)
    {
        if (IsLanding(ws))
            throw new DomainException("Domain_Config_LandingMustBePublic");
    }

    public void EnsureCanDelete(Workspace ws)
    {
        if (IsLanding(ws))
            throw new DomainException("Domain_Workspace_LandingDelete");
    }

    public void Forget(Workspace ws)
    {
        if (LastWorkspaceId == ws.Id)
            LastWorkspaceId = null;
    }

    private static byte[] hash(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hashSize);
}
