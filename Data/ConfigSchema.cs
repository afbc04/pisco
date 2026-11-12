using Npgsql;

namespace Pisco.Data;

public static class ConfigSchema
{
    const string scriptFile = "sql/config.sql";

    public static async Task EnsureCreatedAsync(NpgsqlDataSource db)
    {
        await using var cmd = db.CreateCommand(await readScriptAsync());
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<string> readScriptAsync()
    {
        await using var stream = typeof(ConfigSchema).Assembly.GetManifestResourceStream(scriptFile)
                                 ?? throw new InvalidOperationException("Couldn't initialize the pisco_config schema");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
