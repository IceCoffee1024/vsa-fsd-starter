using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;

/// <summary>
/// Creates independently owned, open SQLite connections with foreign-key enforcement enabled.
/// </summary>
public sealed class SqliteConnectionFactory
{
    /// <summary>
    /// The default number of seconds a command waits to obtain a SQLite lock.
    /// </summary>
    public const int DefaultBusyTimeoutSeconds = 30;

    /// <summary>
    /// Creates a factory for one database path; pooling and lock-wait behavior can be adjusted for the deployment environment.
    /// </summary>
    /// <param name="databasePath">The SQLite database file path.</param>
    /// <param name="pooling">Whether provider connection pooling is enabled.</param>
    /// <param name="busyTimeoutSeconds">The lock-wait timeout in seconds; zero waits indefinitely.</param>
    /// <exception cref="ArgumentException">The database path is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The busy timeout is negative.</exception>
    public SqliteConnectionFactory(
        string databasePath,
        bool pooling = true,
        int busyTimeoutSeconds = DefaultBusyTimeoutSeconds)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("A database path is required.", nameof(databasePath));
        }

        if (busyTimeoutSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(busyTimeoutSeconds),
                "The busy timeout cannot be negative.");
        }

        ConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            DefaultTimeout = busyTimeoutSeconds,
            ForeignKeys = true,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = pooling,
        }.ToString();
    }

    public string ConnectionString { get; }

    /// <summary>
    /// Opens a new connection; the caller owns and must dispose the returned instance.
    /// </summary>
    public async Task<SqliteConnection> OpenConnectionAsync(
        CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
