using System;
using System.IO;
using BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;
using BackendVsaOwin.Modules.Customers.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendVsaOwin.Modules.Customers.Tests.Infrastructure;

internal sealed class CustomerSqliteDatabase : IDisposable
{
    private readonly string _directory;

    public CustomerSqliteDatabase()
    {
        _directory = Path.Combine(
            Path.GetTempPath(),
            "BackendVsaOwin.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        var factory = new SqliteConnectionFactory(
            Path.Combine(_directory, "customers.db"),
            pooling: false);
        SqliteMigrationRunner.Migrate(
            factory,
            new[] { CustomersModule.Assembly },
            NullLoggerFactory.Instance);
        Store = new SqliteCustomerStore(factory);
    }

    public SqliteCustomerStore Store { get; }

    public void Dispose()
    {
        Directory.Delete(_directory, recursive: true);
    }
}
