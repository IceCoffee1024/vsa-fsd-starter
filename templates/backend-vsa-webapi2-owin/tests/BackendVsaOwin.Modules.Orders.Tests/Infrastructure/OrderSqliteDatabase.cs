using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;
using BackendVsaOwin.Modules.Customers;
using BackendVsaOwin.Modules.Orders.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendVsaOwin.Modules.Orders.Tests.Infrastructure;

internal sealed class OrderSqliteDatabase : IDisposable
{
    private readonly string _directory;

    public OrderSqliteDatabase(bool migrate = true)
    {
        _directory = Path.Combine(
            Path.GetTempPath(),
            "BackendVsaOwin.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        ConnectionFactory = new SqliteConnectionFactory(
            Path.Combine(_directory, "orders.db"),
            pooling: false);
        Store = new SqliteOrderStore(ConnectionFactory);

        if (migrate)
        {
            Migrate();
        }
    }

    public SqliteConnectionFactory ConnectionFactory { get; }

    public SqliteOrderStore Store { get; }

    public void Migrate(ILoggerFactory? loggerFactory = null)
    {
        SqliteMigrationRunner.Migrate(
            ConnectionFactory,
            new[]
            {
                CustomersModule.Assembly,
                OrdersModule.Assembly,
            },
            loggerFactory ?? NullLoggerFactory.Instance);
    }

    public async Task AddCustomerAsync(
        Guid id,
        string displayName,
        CancellationToken cancellationToken)
    {
        using var connection = await ConnectionFactory.OpenConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO customers (id, display_name) VALUES (@id, @displayName);";
        command.Parameters.AddWithValue("@id", id.ToString("D"));
        command.Parameters.AddWithValue("@displayName", displayName);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<string[]> GetAppliedScriptsAsync(
        CancellationToken cancellationToken)
    {
        using var connection = await ConnectionFactory.OpenConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ScriptName FROM SchemaVersions ORDER BY ScriptName;";
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var scriptNames = new System.Collections.Generic.List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            scriptNames.Add(reader.GetString(0));
        }

        return scriptNames.ToArray();
    }

    public async Task<bool> TableExistsAsync(
        string tableName,
        CancellationToken cancellationToken)
    {
        using var connection = await ConnectionFactory.OpenConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name;";
        command.Parameters.AddWithValue("@name", tableName);
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    public void Dispose()
    {
        Directory.Delete(_directory, recursive: true);
    }
}
