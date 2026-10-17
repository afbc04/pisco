using System.Globalization;
using System.Text;
using Dapper;
using Npgsql;
using Pisco.Data;
using Pisco.Domain;

namespace Pisco.Services;

public class WorkspaceService(NpgsqlDataSource db)
{
    private const string SchemaPrefix = "ws_";
    private const string WorkspaceColumns = "id, name, schema_name, is_demo, is_landing";

    public async Task<List<Workspace>> ListForUserAsync(long userId, bool isAdmin)
    {
        await using var conn = await db.OpenConnectionAsync();
        var list = await conn.QueryAsync<Workspace>($"""
            SELECT {WorkspaceColumns}
            FROM core.workspaces w
            WHERE CASE
                WHEN @userId = 0 THEN w.is_landing
                WHEN @isAdmin THEN true
                ELSE EXISTS (SELECT 1 FROM core.workspace_members m WHERE m.workspace_id = w.id AND m.user_id = @userId)
            END
            ORDER BY w.name
            """, new { userId, isAdmin });
        return list.AsList();
    }

    public async Task<List<Workspace>> ListAllAsync()
    {
        await using var conn = await db.OpenConnectionAsync();
        var list = await conn.QueryAsync<Workspace>($"SELECT {WorkspaceColumns} FROM core.workspaces ORDER BY name");
        return list.AsList();
    }

    public async Task<long?> GetLastWorkspaceIdAsync(long userId)
    {
        await using var conn = await db.OpenConnectionAsync();
        return await conn.ExecuteScalarAsync<long?>(
            "SELECT last_workspace_id FROM core.users WHERE id = @userId", new { userId });
    }

    public async Task SaveLastWorkspaceAsync(long userId, long workspaceId)
    {
        await using var conn = await db.OpenConnectionAsync();
        await conn.ExecuteAsync(
            "UPDATE core.users SET last_workspace_id = @workspaceId WHERE id = @userId", new { userId, workspaceId });
    }

    public async Task<Workspace> CreateAsync(string name, bool isDemo, bool isLanding)
    {
        var ws = new Workspace { Name = ValidName(name), IsDemo = isDemo, IsLanding = isLanding };

        await using var conn = await db.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        var baseName = SchemaPrefix + Slug(ws.Name);
        ws.SchemaName = baseName;
        for (var i = 2; await conn.ExecuteScalarAsync<bool>(
                 "SELECT EXISTS (SELECT 1 FROM core.workspaces WHERE schema_name = @SchemaName)", ws, tx); i++)
            ws.SchemaName = $"{baseName}_{i}";

        await WorkspaceSchema.CreateAsync(conn, ws.SchemaName, tx);
        if (isLanding)
            await conn.ExecuteAsync("UPDATE core.workspaces SET is_landing = false WHERE is_landing", transaction: tx);
        ws.Id = await conn.ExecuteScalarAsync<long>(
            "INSERT INTO core.workspaces (name, schema_name, is_demo, is_landing) VALUES (@Name, @SchemaName, @IsDemo, @IsLanding) RETURNING id",
            ws, tx);
        await tx.CommitAsync();
        return ws;
    }

    private static string ValidName(string name)
    {
        name = name.Trim();
        if (name.Length == 0)
            throw new DomainException("Domain_Name_Empty");
        if (name.Length > Workspace.NameLengthMax)
            throw new DomainException("Domain_Name_TooLong", Workspace.NameLengthMax);
        return name;
    }

    private static string Slug(string name)
    {
        var sb = new StringBuilder();
        foreach (var c in name.Normalize(NormalizationForm.FormD).ToLowerInvariant())
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9') sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '_') sb.Append('_');
        }
        var slug = sb.ToString().Trim('_');
        if (slug.Length == 0) slug = "ws";
        return slug[..Math.Min(slug.Length, Workspace.SchemaNameLengthMax - SchemaPrefix.Length - 4)];
    }
}
