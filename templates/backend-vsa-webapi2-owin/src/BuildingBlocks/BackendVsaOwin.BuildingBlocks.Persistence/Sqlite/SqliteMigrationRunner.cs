using System;
using System.Collections.Generic;
using System.Reflection;
using DbUp;
using Microsoft.Extensions.Logging;

namespace BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;

/// <summary>
/// Applies embedded module migrations to one SQLite database in the supplied assembly order.
/// </summary>
public static class SqliteMigrationRunner
{
    /// <summary>
    /// Applies each assembly's pending embedded SQL scripts in enumeration order, records them in the shared DbUp journal,
    /// and forwards migration diagnostics to the supplied logging pipeline.
    /// </summary>
    /// <param name="connectionFactory">The connection factory for the database being upgraded.</param>
    /// <param name="migrationAssemblies">Module assemblies in dependency-safe migration order.</param>
    /// <param name="loggerFactory">The application logging pipeline that receives DbUp events.</param>
    /// <exception cref="InvalidOperationException">A migration failed; the original provider exception is available as the inner exception.</exception>
    public static void Migrate(
        SqliteConnectionFactory connectionFactory,
        IEnumerable<Assembly> migrationAssemblies,
        ILoggerFactory loggerFactory)
    {
        if (connectionFactory is null)
        {
            throw new ArgumentNullException(nameof(connectionFactory));
        }

        if (migrationAssemblies is null)
        {
            throw new ArgumentNullException(nameof(migrationAssemblies));
        }

        if (loggerFactory is null)
        {
            throw new ArgumentNullException(nameof(loggerFactory));
        }

        foreach (var assembly in migrationAssemblies)
        {
            var upgrader = DeployChanges.To
                .SqliteDatabase(connectionFactory.ConnectionString)
                .WithScriptsEmbeddedInAssembly(
                    assembly,
                    scriptName => scriptName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                .LogTo(loggerFactory)
                .Build();
            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                throw new InvalidOperationException(
                    $"Database migration failed for assembly '{assembly.FullName}'.",
                    result.Error);
            }
        }
    }
}
