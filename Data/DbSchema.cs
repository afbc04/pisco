using Npgsql;

namespace Pisco.Data;

public static class DbSchema
{
    static readonly string[] scriptFiles = ["sql/config.sql", "sql/finance.sql"];

    public static async Task EnsureCreatedAsync(NpgsqlDataSource db)
    {
        foreach (var file in scriptFiles)
        {
            await using var cmd = db.CreateCommand(await readScriptAsync(file));
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task<string> readScriptAsync(string file)
    {
        await using var stream = typeof(DbSchema).Assembly.GetManifestResourceStream(file)
                                 ?? throw new InvalidOperationException($"Couldn't find the database script {file}");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
