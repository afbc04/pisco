using Dapper;
using Npgsql;

namespace Pisco.Data;

public static class WorkspaceSchema
{
    public static Task CreateAsync(NpgsqlConnection conn, string schema, NpgsqlTransaction? tx = null) =>
        conn.ExecuteAsync($"CREATE SCHEMA IF NOT EXISTS {Quote(schema)}", transaction: tx);

    public static async Task<NpgsqlConnection> OpenAsync(NpgsqlDataSource db, string schema)
    {
        var conn = await db.OpenConnectionAsync();
        await conn.ExecuteAsync($"SET search_path TO {Quote(schema)}");
        return conn;
    }

    private static string Quote(string schema) => $"\"{schema.Replace("\"", "\"\"")}\"";
}
