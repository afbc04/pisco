using Npgsql;

namespace Pisco.Data;

public static class CoreDatabase
{
    const string file_of_core_schema = "sql/core.sql";

    public static async Task EnsureCoreIsInitializatedAsync(NpgsqlDataSource db)
    {
        await using var cmd = db.CreateCommand(await CoreInitializationReadAsync());
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<string> CoreInitializationReadAsync()
    {
        await using var stream = typeof(CoreDatabase).Assembly.GetManifestResourceStream(file_of_core_schema) ?? throw new InvalidOperationException("Couldn't initialize core schema");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
