namespace Pisco.Domain;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = "";
    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? LastWorkspaceId { get; set; }
}
