using System;
using Microsoft.Data.Sqlite;

namespace BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;

/// <summary>
/// Applies database-level SQLite settings that must be established before migrations and application traffic.
/// </summary>
public static class SqliteDatabaseInitializer
{
    /// <summary>
    /// Enables persistent write-ahead logging and fails startup when the database cannot enter WAL mode.
    /// </summary>
    public static void Initialize(SqliteConnectionFactory connectionFactory)
    {
        if (connectionFactory is null)
        {
            throw new ArgumentNullException(nameof(connectionFactory));
        }

        using var connection = new SqliteConnection(connectionFactory.ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode = WAL;";
        var journalMode = Convert.ToString(command.ExecuteScalar());
        if (!string.Equals(journalMode, "wal", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"SQLite did not enter WAL journal mode; actual mode: '{journalMode ?? "<null>"}'.");
        }
    }
}
